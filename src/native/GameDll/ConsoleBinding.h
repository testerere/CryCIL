#pragma once
#include "MonoAPIBinding.h"
#include "CemonoCVar.h"
#include "IConsole.h"

class CConsoleBinding : public MonoAPIBinding
{
public:
	CConsoleBinding();
	virtual ~CConsoleBinding();

protected:
	virtual const char* GetClassName() { return "Console"; }
	virtual const char* GetNamespaceExtension() { return "API"; }

	static bool _GetCVar(MonoString* cvarName);
	static MonoString* _GetCVarHelpText(MonoString* cvarName);
	static int _GetCVarValueInt(MonoString* cvarName);
	static float _GetCVarValueFloat(MonoString* cvarName);
	static MonoString* _GetCVarValueString(MonoString* cvarName);
	static void _SetCVarValueInt(MonoString* cvarName, int value);
	static void _SetCVarValueFloat(MonoString* cvarName, float value);
	static void _SetCVarValueString(MonoString* cvarName, MonoString* value);
	static int _GetCVarFlags(MonoString* cvarName);
	static void _SetCVarFlags(MonoString* cvarName, int value);
private:
	static ICVar* GetCvarByName(MonoString* cvarName);
};

