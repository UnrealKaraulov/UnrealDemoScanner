#include <amxmodx>
#include <fakemeta>
#include <engine>
#include <reapi>

#define PLUGIN "Unreal Demo Plugin"
#define AUTHOR "karaulov"
#define VERSION "1.0"

public plugin_init() 
{
	register_plugin(PLUGIN, VERSION, AUTHOR);
	register_clcmd("fullupdate", "UnrealDemoHelpInitialize");
	RegisterHookChain(RG_PM_Move, "PM_Move")
	//register_forward(FM_PlaybackEvent, "fw_PlaybackEvent", 1)	
}

/*
Server not processed angles. Always empty.*/
public pfn_playbackevent(flags, entid, eventid, Float:delay, Float:Origin[3], Float:Angles[3], Float:fparam1, Float:fparam2, iparam1, iparam2, bparam1, bparam2)
{
	if(entid > 0 && entid < 33)
	{
		WriteDemoInfo(entid, "UDS/EVENT/1");
	}
}

public PM_Move(const id)
{
	new button = get_entvar(id, var_button)
	new oldbuttons = get_entvar(id, var_oldbuttons)
	if ((button & IN_ATTACK) && !(oldbuttons & IN_ATTACK))
	{
		new cmdx = get_pmove( pm_cmd );
		
		/*
		Can't compare angles because client has 4byte float, server smaller.(Delta encoded)
		new Float:vAngles[3];
		get_ucmd(cmdx, ucmd_viewangles, vAngles);
		WriteDemoInfo(id, "UDS/ANGLE/%i", vAngles[1]);
		*/
		
		// DETECT FAKE LAG 
		WriteDemoInfo(id, "UDS/UCMD/%i/%i", get_ucmd(cmdx, ucmd_lerp_msec), get_ucmd(cmdx, ucmd_msec));
	}
}

public UnrealDemoHelpInitialize(id) 
{
	new szAuth[64];
	get_user_authid(id,szAuth,charsmax(szAuth));
	WriteDemoInfo(id,"UDS/AUTH/%s",szAuth);
	
	new szDate[64];
	get_time( "%d.%m.%Y %H:%M:%S", szDate, charsmax( szDate ) );
	WriteDemoInfo(id,"UDS/DATE/%s",szDate);
	
	return PLUGIN_HANDLED;
}

// SVC_RESOURCELOCATION ignore all strings not started with http or https
// and can be used to save any info to demo
public WriteDemoInfo(const index, const message[], any:... )
{
	new buffer[ 256 ];
	new numArguments = numargs();
	
	if (numArguments == 2)
	{
		message_begin(MSG_ONE, SVC_RESOURCELOCATION, _, index)
		write_string(message)
		message_end()
	}
	else 
	{
		vformat( buffer, charsmax( buffer ), message, 3 );
		message_begin(MSG_ONE, SVC_RESOURCELOCATION, _, index)
		write_string(buffer)
		message_end()
	}
}