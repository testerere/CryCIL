#include "StdAfx.h"
#include "MonoInput.h"

#include "MonoScriptSystem.h"

#include <IGameFramework.h>

#include <IMonoAssembly.h>
#include <IMonoClass.h>
#include <IMonoArray.h> 

TActionHandler<CScriptbind_Input>	CScriptbind_Input::s_actionHandler;

CScriptbind_Input::CScriptbind_Input()
{
	REGISTER_METHOD(RegisterAction);

	static_cast<CScriptSystem *>(GetMonoScriptSystem())->GetIGameFramework()->GetIActionMapManager()->AddExtraActionListener(this);
	gEnv->pHardwareMouse->AddListener(this);

	if(gEnv->pInput)
		gEnv->pInput->AddEventListener(this);
}

CScriptbind_Input::~CScriptbind_Input()
{
	// The code below currently crashes the Launcher at shutdown
	/*if(static_cast<CScriptSystem *>(GetMonoScriptSystem())->GetIGameFramework())
	{
		if(IActionMapManager *pActionmapManager = static_cast<CScriptSystem *>(GetMonoScriptSystem())->GetIGameFramework()->GetIActionMapManager())
			pActionmapManager->RemoveExtraActionListener(this);
	}*/

	if(gEnv->pHardwareMouse)
		gEnv->pHardwareMouse->RemoveListener(this);

	if(gEnv->pInput)
		gEnv->pInput->RemoveEventListener(this);
}

IMonoClass *CScriptbind_Input::GetClass()
{
	return GetMonoScriptSystem()->GetCryBraryAssembly()->GetClass("Input");
}

void CScriptbind_Input::OnHardwareMouseEvent(int iX,int iY,EHARDWAREMOUSEEVENT eHardwareMouseEvent, int wheelDelta)
{
	IMonoArray *pParams = CreateMonoArray(4);
	pParams->Insert(iX);
	pParams->Insert(iY);
	pParams->Insert(eHardwareMouseEvent);
	pParams->Insert(wheelDelta);

	IMonoClass *pInputClass = GetMonoScriptSystem()->GetCryBraryAssembly()->GetClass("Input", "CryEngine");
	pInputClass->GetMethod("OnMouseEvent", 4)->InvokeArray(NULL, pParams);
	SAFE_RELEASE(pParams);
}

bool CScriptbind_Input::OnInputEvent(const SInputEvent &event)
{
	IMonoArray *pParams = CreateMonoArray(2);
	pParams->Insert(event.keyName.c_str());
	pParams->Insert(event.value);

	IMonoClass *pInputClass = GetMonoScriptSystem()->GetCryBraryAssembly()->GetClass("Input", "CryEngine");
	pInputClass->GetMethod("OnKeyEvent", 2)->InvokeArray(NULL, pParams);
	SAFE_RELEASE(pParams);

	return false;
}

void CScriptbind_Input::OnAction(const ActionId& actionId, int activationMode, float value)
{
	s_actionHandler.Dispatch(this, 0, actionId, activationMode, value);
}

bool CScriptbind_Input::OnActionTriggered(EntityId entityId, const ActionId& actionId, int activationMode, float value)
{
	IMonoArray *pParams = CreateMonoArray(3);
	pParams->Insert(actionId.c_str());
	pParams->Insert(activationMode);
	pParams->Insert(value);

	IMonoClass *pInputClass = GetMonoScriptSystem()->GetCryBraryAssembly()->GetClass("Input", "CryEngine");
	pInputClass->GetMethod("OnActionTriggered", 3)->InvokeArray(NULL, pParams);
	SAFE_RELEASE(pParams);

	return false;
}

// Scriptbinds
void CScriptbind_Input::RegisterAction(mono::string actionName)
{
	if(!s_actionHandler.GetHandler(ActionId(ToCryString(actionName))))
		s_actionHandler.AddHandler(ActionId(ToCryString(actionName)), &CScriptbind_Input::OnActionTriggered);
}