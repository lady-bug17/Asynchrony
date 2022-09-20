#include <SFML/Graphics.hpp>
#include <iostream>
#include <thread>
#include <vector>
#include <mutex>
#include <condition_variable>

#define Love 0;
using namespace std;
using namespace sf;

RenderWindow window(sf::VideoMode(1800, 400), "Orange", Style::Default);
Texture pearTexture;
Sprite pear;
Texture gifTexture;
Sprite gif;
Texture bananaTexture;
Sprite banana;
CircleShape orange(100.f);
Texture orangeTexture;
Font font;
bool forThread[4];
mutex m[4];
condition_variable cv[4];
bool isRunning = true;

void stopThread(bool& a)
{
    a = true;
}

void resumeThread(bool& a, condition_variable& cv)
{
    a = false;
    cv.notify_all();
}

class Button
{
public:
    RectangleShape body;
    bool* isClicked;
    Text text;
    condition_variable* cv;

    Button()
    {
        body.setPosition(0, 0);
        bool a = false;
        isClicked = &a;
        text.setString("Stop");
        text.setFont(font);
    }

    Button(int x, int y, bool& a, condition_variable& c)
    {
        body.setPosition(x, y);
        body.setSize(Vector2f(150, 60));
        body.setFillColor(Color(230, 230, 230));
        text.setFont(font);
        text.setString("Stop");
        text.setPosition(Vector2f(x + 25, y + 15));
        text.setFillColor(Color::Red);
        isClicked = &a;
        cv = &c;
    }

    void draw()
    {
        window.draw(body);
        window.draw(text);
    }

    void mouseClick(RenderWindow& window)
    {
        if (Mouse::getPosition(window).x >= body.getPosition().x &&
            Mouse::getPosition(window).y >= body.getPosition().y &&
            Mouse::getPosition(window).x <= body.getPosition().x + body.getSize().x &&
            Mouse::getPosition(window).y <= body.getPosition().y + body.getSize().y)
        {
            if (Mouse::isButtonPressed && text.getString() == "Stop")
            {
                text.setString("Resume");
                stopThread(*isClicked);
            }
            else if (Mouse::isButtonPressed)
            {
                text.setString("Stop");
                resumeThread(*isClicked, *cv);
            }
        }
    }

};

void bananaFunc()
{
    bananaTexture.loadFromFile("banana.png");
    banana.setTexture(bananaTexture);
    banana.setOrigin(146, 89.25);
    banana.setPosition(112, 150);
    banana.scale(0.5, 0.5);
    cout << banana.getGlobalBounds().width << " " << banana.getGlobalBounds().height;
}

void orangeFunc()
{
    orange.setOrigin(100, 100);
    orange.setPosition(1575, 200);
    orangeTexture.loadFromFile("orange.png");
    orange.setTexture(&orangeTexture);
}

void gifFunc()
{
    gifTexture.loadFromFile("1.gif");
    gif.setTexture(gifTexture);
    gif.setScale(0.85, 0.9);
}

void pearFunc()
{
    pearTexture.loadFromFile("pear.png");
    pear.setTexture(pearTexture);
    pear.setScale(0.05, 0.05);
    pear.setPosition(655, 225);
}

void changeBanana()
{
    bool isRising = true;
    while (isRunning)
    {
        unique_lock<mutex> ul(m[0]);
        cv[0].wait(ul, []() {return !forThread[0];});
        if (isRising)
        {
            banana.scale(1.0000005, 1.0000005);
            if (banana.getGlobalBounds().width > 300)
            {
                isRising = false;
            }
        }
        else
        {
            banana.scale(0.9999995, 0.9999995);
            if (banana.getGlobalBounds().width < 100)
            {
                isRising = true;
            }
        }
    }
}

void changeOrange()
{
    while (isRunning)
    {
        unique_lock<mutex> ul(m[1]);
        cv[1].wait(ul, []() {return !forThread[1];});
        orange.rotate(0.00005);
    }
}

void changeGif()
{
    int counter = 0;
    while (isRunning)
    {
        unique_lock<mutex> ul(m[2]);
        cv[2].wait(ul, []() {return !forThread[2];});
        counter++;
        counter %= 13;
        gifTexture.loadFromFile(to_string(counter) + ".gif");
        gif.setTexture(gifTexture);
        gif.setPosition(900, 0);
    }
}

void changePear()
{
    while (isRunning)
    {
        while (pear.getGlobalBounds().top > 50)
        {
            unique_lock<mutex> ul(m[3]);
            cv[3].wait(ul, []() {return !forThread[3];});
            pear.setPosition(pear.getGlobalBounds().left, pear.getGlobalBounds().top - 1);
            this_thread::sleep_for(chrono::milliseconds(10));
        }
        while (pear.getGlobalBounds().left > 400)
        {
            unique_lock<mutex> ul(m[3]);
            cv[3].wait(ul, []() {return !forThread[3];});
            pear.setPosition(pear.getGlobalBounds().left - 1, pear.getGlobalBounds().top);
            this_thread::sleep_for(chrono::milliseconds(10));
        }
        while (pear.getGlobalBounds().top < 225)
        {
            unique_lock<mutex> ul(m[3]);
            cv[3].wait(ul, []() {return !forThread[3];});
            pear.setPosition(pear.getGlobalBounds().left, pear.getGlobalBounds().top + 1);
            this_thread::sleep_for(chrono::milliseconds(10));
        }
        while (pear.getGlobalBounds().left < 655)
        {
            unique_lock<mutex> ul(m[3]);
            cv[3].wait(ul, []() {return !forThread[3];});
            pear.setPosition(pear.getGlobalBounds().left + 1, pear.getGlobalBounds().top);
            this_thread::sleep_for(chrono::milliseconds(10));
        }
    }
}



int main()
{
    vector<thread> threads;

    font.loadFromFile("Roboto-Black.ttf");

    orangeFunc();
    bananaFunc();
    gifFunc();
    pearFunc();

    Button b1(0, 0, forThread[0], cv[0]);
    Button b2(450, 0, forThread[3], cv[3]);
    Button b3(900, 0, forThread[2], cv[2]);
    Button b4(1350, 0, forThread[1], cv[1]);


    threads.push_back(thread(changePear));
    threads.push_back(thread(changeGif));
    threads.push_back(thread(changeBanana));
    threads.push_back(thread(changeOrange));

    for (auto& t : threads)
    {
        t.detach();
    }

    while (window.isOpen())
    {
        Event event;
        while (window.pollEvent(event))
        {
            if (event.type == sf::Event::Closed)
            {
                window.close();
                isRunning = false;
            }
            if (event.type == sf::Event::MouseButtonPressed)
            {
                b1.mouseClick(window);
                b2.mouseClick(window);
                b3.mouseClick(window);
                b4.mouseClick(window);
            }
        }
        window.clear(Color::Yellow);
        window.draw(pear);
        window.draw(gif);
        window.draw(orange);
        window.draw(banana);
        b1.draw();
        b2.draw();
        b3.draw();
        b4.draw();
        window.display();
    }
    this_thread::sleep_for(chrono::seconds(1));
    return Love;
}
