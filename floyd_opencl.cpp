#include <iostream>
#include <thread>
#include <ctime>
#include <CL/cl.h>
using namespace std;

void floyd(int** matrix, int** ways, unsigned size, unsigned from, unsigned to)
{
	for (unsigned k = from; k < to; ++k)
	{
		for (unsigned i = 0; i < size; ++i)
		{
			for (unsigned j = 0; j < size; ++j)
			{
				if (matrix[i][k] + matrix[k][j] < matrix[i][j] && i != k && j != k)
				{
					matrix[i][j] = matrix[i][k] + matrix[k][j];
					ways[i][j] = k - 1;
				}
			}
		}
	}
}

void createThreads(int** matrix, int** ways, unsigned size, unsigned threadCount = 1)
{
	thread* threadArray = new thread[threadCount];
	unsigned from = 0;
	unsigned threadStep = size / threadCount;
	unsigned to = threadStep;
	for (unsigned i = 0; i < threadCount; ++i)
	{
		threadArray[i] = thread(floyd, matrix, ways, size, from, to);
		from += threadStep;
		to += threadStep;
	}
	for (unsigned i = 0; i < threadCount; ++i)
	{
		if (threadArray[i].joinable())
		{
			threadArray[i].join();
		}
	}
}

void floydOpenCL(int** matrix, int** ways, unsigned size)
{
	const char *source_code =
		"__kernel void OpenCLFloyd(__global uint * distanceBuffer, __global uint * verBuffer, const unsigned int quantity, const unsigned int thread) \n"\
		"{ \n"\
			"int first = get_global_id(0); \n"\
			"int second = get_global_id(1); \n"\
			"int num = thread; \n"\
			"int oldWeight = distanceBuffer[second * quantity + first]; \n"\
			"int newWeight = (distanceBuffer[second * quantity + num] + distanceBuffer[num * quantity + first]); \n"\
			"if (newWeight < oldWeight){ \n"\
				"distanceBuffer[second * quantity + first] = newWeight; \n"\
			" } \n"\
		"} \n"\
		"\n";
	int* distanceMatrix = new int[size*size];
	int counter = 0;
	for (int i = 0; i < size; ++i)
	{
		for (int j = 0; j < size; ++j)
		{
			distanceMatrix[++counter] = matrix[i][j];
		}
	}
	int* verMatrix = new int[size*size];
	for (int i = 0; i<size; ++i)
	{
		for (int j = 0; j<size; j++)
		{
			ways[i][j] = distanceMatrix[i*size + j];
		}
	}

	for (int i = 0; i < size; ++i)
	{
		for (int j = 0; j < i; ++j)
		{
			verMatrix[i * size + j] = i;
			verMatrix[j * size + i] = j;
		}
		verMatrix[i * size + i] = i;
	}
	cl_platform_id platformIdentifier;
	cl_device_id deviceIdentifier;
	cl_uint platformsQuantity = 0;
	cl_uint deviceQuantity = 0;
	if (clGetPlatformIDs(1, &platformIdentifier, &platformsQuantity) != CL_SUCCESS)
	{
		cout << "Unable to get platform id" << endl;
	}
	if (clGetDeviceIDs(platformIdentifier, CL_DEVICE_TYPE_CPU, 1, &deviceIdentifier, &deviceQuantity) != CL_SUCCESS)
	{
		cout << "Unable to get deviceIdentifier" << endl;
	}
	cl_context_properties properties[3];
	properties[0] = CL_CONTEXT_PLATFORM;
	properties[1] = (cl_context_properties)platformIdentifier;
	properties[2] = 0;

	cl_int err;
	cl_context context = clCreateContext(properties, 1, &deviceIdentifier, NULL, NULL, &err);
	cl_command_queue commandQueue = clCreateCommandQueue(context, deviceIdentifier, 0, &err);
	cl_program program = clCreateProgramWithSource(context, 1, (const char **)&source_code, NULL, &err);

	if (clBuildProgram(program, 0, NULL, NULL, NULL, NULL) != CL_SUCCESS)
	{
		printf("Error building program\n");
	}

	cl_kernel kernel = clCreateKernel(program, "OpenCLFloyd", &err);
	cl_mem distanceMatrixBuffer = clCreateBuffer(context, CL_MEM_READ_WRITE, sizeof(int) * size * size, NULL, NULL);
	cl_mem verMatrixBuffer = clCreateBuffer(context, CL_MEM_READ_WRITE, sizeof(int) * size * size, NULL, NULL);
	clEnqueueWriteBuffer(commandQueue, distanceMatrixBuffer, CL_TRUE, 0, sizeof(int) * size * size, distanceMatrix, 0, NULL, NULL);
	clEnqueueWriteBuffer(commandQueue, verMatrixBuffer, CL_TRUE, 0, sizeof(int) * size * size, verMatrix, 0, NULL, NULL);

	int arraySize = size;
	clSetKernelArg(kernel, 0, sizeof(cl_mem), &distanceMatrixBuffer);
	clSetKernelArg(kernel, 1, sizeof(cl_mem), &verMatrixBuffer);
	clSetKernelArg(kernel, 2, sizeof(int), &arraySize);
	clSetKernelArg(kernel, 3, sizeof(int), &arraySize);

	size_t *global = new size_t[2];
	global[0] = size;
	global[1] = size;

	size_t *local = new size_t[2];
	int intSize = 4;
	local[0] = intSize;
	local[1] = intSize;
	int quantityPas = size;
	clock_t beginTime = clock();
	for (int i = 0; i < quantityPas; ++i)
	{
		clSetKernelArg(kernel, 3, sizeof(int), &i);
		clEnqueueNDRangeKernel(commandQueue, kernel, 2, NULL, global, local, 0, NULL, NULL);
		clFlush(commandQueue);
	}
	clFinish(commandQueue);
	cout << "OpenCL Floyd time: " << ((float)(clock() - beginTime)) / CLOCKS_PER_SEC  << endl;
	clEnqueueReadBuffer(commandQueue, distanceMatrixBuffer, CL_TRUE, 0, sizeof(int) *size * size, distanceMatrix, 0, NULL, NULL);
	clEnqueueReadBuffer(commandQueue, verMatrixBuffer, CL_TRUE, 0, sizeof(int) * size * size, verMatrix, 0, NULL, NULL);
	clReleaseMemObject(distanceMatrixBuffer);
	clReleaseMemObject(verMatrixBuffer);
	clReleaseProgram(program);
	clReleaseKernel(kernel);
	clReleaseCommandQueue(commandQueue);
	clReleaseContext(context);
}

void main()
{
	const unsigned size = 1000;
	int** matrix = new  int*[size];
	int** ways = new  int*[size];
	srand(time(NULL));
	for (unsigned i = 0; i < size; ++i)
	{
		matrix[i] = new int[size];
		ways[i] = new int[size];
		for (unsigned j = 0; j < size; ++j)
		{
			if (i == j)
			{
				matrix[i][j] = 0;
			}
			matrix[i][j] = rand() % 100;
		}
	}
	clock_t beginTime = clock();
	floyd(matrix, ways, size, 0, size);
	cout << "Simple Floyd time: " << (float)(clock() - beginTime) / CLOCKS_PER_SEC << endl;

	beginTime = clock();
	createThreads(matrix, ways, size, 4);
	cout << "Parallel Floyd time(4 threads): " << (float)(clock() - beginTime) / CLOCKS_PER_SEC << endl;
	
	floydOpenCL(matrix, ways, size);
	system("pause");
}
