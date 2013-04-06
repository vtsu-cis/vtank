/*!
    \file asynctemplate.hpp
    \brief Template class that makes it easier to use AMI callbacks.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef ASYNCTEMPLATE
#define ASYNCTEMPLATE

typedef void ( *response_callback_t )( );
typedef void ( *exception_callback_t )(const Ice::Exception& ex);
typedef void ( *player_exception_callback_t )(const int id, const Ice::Exception &ex);

/*!
    In Slice, every method marked with the "ami" tag can send asynchronous
    messages through a class. However, each method must use it's own class
    for every single type of message delivery. This can get very tedious to
    define for every method.

    Instead, a template class is created that supports any callback method
    whose return type is 'void'. Regardless, you may still set their own
    function callback in case they're interested in knowing when the user
    responded. You may also set an exception callback just in case a user
    disconnects.
*/
template<class T>
class VoidAsyncCallback : public T
{
private:
    exception_callback_t exception_callback;
    response_callback_t response_callback;

public:
    VoidAsyncCallback(const exception_callback_t ex = NULL, const response_callback_t reply = NULL) 
    {
        exception_callback = ex;
        response_callback = reply;
    }

    void set_exception_callback(exception_callback_t ex)
    {
        exception_callback = ex;
    }

    void set_response_callback(response_callback_t reply)
    {
        response_callback = reply;
    }

    virtual void ice_exception(const Ice::Exception& ex)
    {
        if (exception_callback != NULL) {
            exception_callback(ex);
        }
    }

    virtual void ice_response()
    {
        if (response_callback != NULL) {
            response_callback();
        }
    }
};

/*!
    The PlayerAsyncCallback class is almost exactly the same as VoidAsyncCallback, except it
    supports handling exceptions for specific players.
*/
template<class T>
class PlayerAsyncCallback : public T
{
private:
    int player_id;
    player_exception_callback_t exception_callback;

public:
    PlayerAsyncCallback(const int id, const player_exception_callback_t ex) 
        : player_id(id), exception_callback(ex)
    {
    }

    virtual void ice_exception(const Ice::Exception& ex)
    {
        if (exception_callback != NULL) {
            exception_callback(player_id, ex);
        }
    }

    virtual void ice_response()
    {}
};

#endif