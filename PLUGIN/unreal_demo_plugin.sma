// ATTENTION! 
// NEED INSTALL THIS PLUGIN AT START OF PLUGINS.INI FILE!!! 

#include <amxmodx>
#include <fakemeta>
#include <engine>
#include <reapi>

#define PLUGIN "Unreal Demo Plugin"
#define AUTHOR "karaulov"
#define VERSION "1.53"


new g_iDemoHelperInitStage[33];
new g_iFrameNum[33];
new g_iPbEventCount[33];

public plugin_init() 
{
	register_plugin(PLUGIN, VERSION, AUTHOR);
	register_cvar( "unreal_demoplug", VERSION, FCVAR_SERVER | FCVAR_SPONLY | FCVAR_UNLOGGED );
	
	register_clcmd("fullupdate", "UnrealDemoHelpInitialize");
	
	RegisterHookChain(RG_PM_Move, "PM_Move")
	RegisterHookChain(RG_CBasePlayer_Jump, "HC_CBasePlayer_Jump_Pre", .post = false);

	register_forward(FM_PlaybackEvent, "fw_PlaybackEvent")	
}


public client_disconnected(id)
{
	g_iPbEventCount[id] = 0;
	g_iFrameNum[id] = 0;
	g_iDemoHelperInitStage[id] = 0;
}

/*Server not processed angles. Always empty.*/
public fw_PlaybackEvent( iFlags, id, eventIndex )
{
	if(id > 0 && id < 33 && g_iDemoHelperInitStage[id] == -1)
	{
		g_iPbEventCount[id]++;
	}
	
	return FMRES_IGNORED;
}


/* Более точное определение прыжка, костыль из-за того что reapi не позволяет узнать что игрок прыгнул */

public HC_CBasePlayer_Jump_Pre(id) 
{
	new iFlags = get_entvar(id,var_flags);
	
	if (g_iDemoHelperInitStage[id] != -1)
	{
		return HC_CONTINUE;
	}
	
	if (iFlags & FL_WATERJUMP)
	{
		return HC_CONTINUE;
	}
	
	if (!(iFlags & FL_ONGROUND))
	{
		return HC_CONTINUE;
	}
	
	if (get_entvar(id,var_waterlevel) >= 2)
	{
		return HC_CONTINUE;
	}
	
	if (!is_entity(get_entvar(id,var_groundentity)))
	{
		return HC_CONTINUE;
	}
	
	if (!(get_member(id,m_afButtonPressed) & IN_JUMP))
	{
		return HC_CONTINUE;
	}
	
	if (get_entvar(id, var_oldbuttons) & IN_JUMP && get_entvar(id, var_button) & IN_JUMP)
		WriteDemoInfo(id, "UDS/JMP/2");
	else 
		WriteDemoInfo(id, "UDS/JMP/1");
	return HC_CONTINUE;
}

public PM_Move(const id)
{
	new button = get_entvar(id, var_button)
	new oldbuttons = get_entvar(id, var_oldbuttons)
	if (g_iDemoHelperInitStage[id] == -1 && !(button & IN_ATTACK) && (oldbuttons & IN_ATTACK))
	{
		new cmdx = get_pmove( pm_cmd );
		
		g_iFrameNum[id]++;
		WriteDemoInfo(id, "UDS/XCMD/%i/%i/%i", get_ucmd(cmdx, ucmd_lerp_msec), get_ucmd(cmdx, ucmd_msec),g_iFrameNum[id]);
		if (g_iPbEventCount[id] > 0)
			WriteDemoInfo(id, "UDS/EVENTS/%i",g_iPbEventCount[id]);
		g_iPbEventCount[id] = 0;
	}
	return HC_CONTINUE;
}

public UnrealDemoHelpInitialize(id) 
{
	g_iPbEventCount[id] = 0;
	g_iFrameNum[id] = 0;
	g_iDemoHelperInitStage[id] = 0;
	if (is_user_connected(id))
	{
		set_task(1.0,"DemoHelperInitializeTask",id);
	}
}

public DemoHelperInitializeTask(id)
{
	if (!is_user_connected(id) || g_iDemoHelperInitStage[id] == -1)
	{
		return;
	}
	g_iDemoHelperInitStage[id]++;
	switch(g_iDemoHelperInitStage[id])
	{
		case 1:
		{
			WriteDemoInfo(id,"UDS/VER/%s",VERSION);
			set_task(1.0,"DemoHelperInitializeTask",id);
		}
		case 2:
		{
			new szAuth[64];
			get_user_authid(id,szAuth,charsmax(szAuth));
			WriteDemoInfo(id,"UDS/AUTH/%s",szAuth);
			
			new szDate[64];
			get_time( "%d.%m.%Y %H:%M:%S", szDate, charsmax( szDate ) );
			WriteDemoInfo(id,"UDS/DATE/%s",szDate);
			set_task(1.0,"DemoHelperInitializeTask",id);
		}
		case 3:
		{
			if (get_cvar_num("sv_minrate") < 25000 || get_cvar_num("sv_maxrate") < 25000)
			{
				WriteDemoInfo(id,"UDS/BAD/1");
			}
			
			if (get_cvar_num("sv_minupdaterate") < 30 || get_cvar_num("sv_maxupdaterate") < 30)
			{
				WriteDemoInfo(id,"UDS/BAD/2");
			}
			g_iPbEventCount[id] = 0;
			g_iDemoHelperInitStage[id] = -1;
		}
	}
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