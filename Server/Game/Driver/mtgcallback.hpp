/*!
	\file mtgcallback.hpp
	\brief 
	\author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef MTGCALLBACK_HPP
#define MTGCALLBACK_HPP

class MTGCallback : public MainToGameSession::ClientSession
{
public:
	MTGCallback();
	~MTGCallback();

	/* The following funtions are implemented by MainToGameSession::ClientSession.
	 * To view documentation, see the MainToGameSession.ice file.
	 */
	virtual void destroy(const Ice::Current& = Ice::Current());
    virtual void KeepAlive(const ::Ice::Current& = ::Ice::Current());
    virtual void AddPlayer(const std::string&, const std::string&, Ice::Int, 
        const VTankObject::TankAttributes&, const Ice::Current& = Ice::Current());
	virtual void RemovePlayer(const std::string&, const Ice::Current& = Ice::Current());
	virtual void ForceMaxPlayerLimit(Ice::Int, const Ice::Current& = Ice::Current());
    virtual void UpdateMapList(const Ice::StringSeq&, const Ice::Current& = Ice::Current());
    virtual Ice::Int GetPlayerLimit(const Ice::Current& = Ice::Current());
	virtual void UpdateUtilities(const VTankObject::UtilityList &,const Ice::Current &);
};

#endif
