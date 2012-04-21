#include "StdAfx.h"
#include "GameRules.h"

#include <IGameRulesSystem.h>
#include <IActorSystem.h>

CGameRules::CGameRules()
{
	REGISTER_METHOD(RegisterGameMode);
	REGISTER_METHOD(AddGameModeAlias);
	REGISTER_METHOD(AddGameModeLevelLocation);
	REGISTER_METHOD(SetDefaultGameMode);

	REGISTER_METHOD(SpawnPlayer);
}

//-----------------------------------------------------------------------------
void CGameRules::RegisterGameMode(mono::string gamemode)
{
	// gEnv->pGameFramework is set too late, so we'll have to set it earlier in CGameStartup::InitFramework. (gEnv->pGameFramework = m_pFramework after the ModuleInitISystem call)
	if(IGameRulesSystem *pGameRulesSystem = gEnv->pGameFramework ? gEnv->pGameFramework->GetIGameRulesSystem() : NULL)
	{
		if(!pGameRulesSystem->HaveGameRules(*gamemode))
			pGameRulesSystem->RegisterGameRules(*gamemode, "GameRules");
	}
}

//-----------------------------------------------------------------------------
void CGameRules::AddGameModeAlias(mono::string gamemode, mono::string alias)
{
	gEnv->pGameFramework->GetIGameRulesSystem()->AddGameRulesAlias(*gamemode, *alias);
}

//-----------------------------------------------------------------------------
void CGameRules::AddGameModeLevelLocation(mono::string gamemode, mono::string location)
{
	gEnv->pGameFramework->GetIGameRulesSystem()->AddGameRulesLevelLocation(*gamemode, *location);
}

//-----------------------------------------------------------------------------
void CGameRules::SetDefaultGameMode(mono::string gamemode)
{
	gEnv->pConsole->GetCVar("sv_gamerulesdefault")->Set(*gamemode);
}

//-----------------------------------------------------------------------------
EntityId CGameRules::GetPlayer()
{
	return gEnv->pGameFramework->GetClientActorId();
}

//-----------------------------------------------------------------------------
EntityId CGameRules::SpawnPlayer(int channelId, mono::string name, mono::string className, Vec3 pos, Vec3 angles, Vec3 scale)
{
	if(gEnv->bServer)
	{
		if(IActor *pActor = gEnv->pGameFramework->GetIActorSystem()->CreateActor(channelId, ToCryString(name), ToCryString(className), pos, Quat(Ang3(angles)), scale))
			return pActor->GetEntityId();
	}

	return 0;
}