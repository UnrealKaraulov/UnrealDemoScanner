// ATTENTION! 
// [WARNING] NEED INSTALL THIS PLUGIN AT START OF PLUGINS.INI FILE!!! 
// [ПРЕДУПРЕЖДЕНИЕ] НЕОБХОДИМО УСТАНОВИТЬ ЭТОТ ПЛАГИН В НАЧАЛО СПИСКА PLUGINS.INI ДЛЯ ПЕРЕХВАТА fullupdate!

#include <amxmodx>
#include <fakemeta>
#include <engine>
#include <reapi>

#define PLUGIN "Unreal Demo Plugin"
#define AUTHOR "karaulov"
#define VERSION "1.63"

// IF NEED REDUCE TRAFFIC USAGE UNCOMMENT THIS LINE
// ЕСЛИ НЕОБХОДИМО БОЛЬШЕ ДЕТЕКТОВ (НО ТАК ЖЕ БОЛЬШЕ ТРАФИКА) ЗАКОММЕНТИРУЙТЕ ЭТУ СТРОКУ
#define SMALL_TRAFFIC

new g_iDemoHelperInitStage[33] = {0,...};
new g_iFrameNum[33] = {0,...};
new g_iJumpCount[33] = {0,...};
new g_bNeedResend[33] = {false,...};

new Float:g_flLastEventTime[33] = {0.0,...};
new Float:g_flLastSendTime[33] = {-1.0,...};
new Float:g_flPMoveTime[33] = {0.0,...};
new Float:g_flStartCmdTime[33] = {0.0,...};
new Float:g_flDelay1Msec[33] = {-1.0,...};
new Float:g_flDelay2Time[33] = {-1.0,...};
new Float:g_flPMovePrevPrevAngles[33][3];
new Float:g_flGameTimeReal = 0.0;

public plugin_init() 
{
	register_plugin(PLUGIN, VERSION, AUTHOR);
	register_cvar("unreal_demoplug", VERSION, FCVAR_SERVER | FCVAR_SPONLY | FCVAR_UNLOGGED);
	
	#if REAPI_VERSION < 524309
	RegisterHookChain(RH_SV_AllowPhysent, "UnrealDemoHelpInitialize", .post = false);
	#else 
	RegisterHookChain(RH_ExecuteServerStringCmd, "UnrealDemoHelpInitialize", .post = false);
	#endif
	
	RegisterHookChain(RG_PM_Move, "PM_Move", .post = false);
	RegisterHookChain(RG_CBasePlayer_Jump, "HC_CBasePlayer_Jump_Pre", .post = false);
	
	register_forward(FM_PlaybackEvent, "fw_PlaybackEvent");

	new plugin_id = get_plugin(-1);
	log_amx("Unreal Demo Plugin Loaded. ID: %i. Author: %s. Version: %s", plugin_id, AUTHOR, VERSION);
	
	if (plugin_id != 0)
	{
		log_error(AMX_ERR_GENERAL, "Error! Unreal Demo Plugin need install at start of plugins.ini file!");
		log_error(AMX_ERR_GENERAL, "Error! Unreal Demo Plugin need install at start of plugins.ini file!");
		log_error(AMX_ERR_GENERAL, "Error! Unreal Demo Plugin need install at start of plugins.ini file!");
	}
}

public server_frame()
{
	g_flGameTimeReal = get_gametime();
}

public client_disconnected(id)
{
	g_flLastEventTime[id] = 0.0;
	g_flLastSendTime[id] = -1.0;
	g_flStartCmdTime[id] = -1.0;
	g_iFrameNum[id] = 0;
	g_iJumpCount[id] = 0;
	g_iDemoHelperInitStage[id] = 0;
	g_flDelay1Msec[id] = -1.0;
	g_flDelay2Time[id] = -1.0;
	g_flPMoveTime[id] = 0.0;
	g_bNeedResend[id] = false;

	g_flPMovePrevPrevAngles[id][0] = g_flPMovePrevPrevAngles[id][1] = g_flPMovePrevPrevAngles[id][2] = 0.0;

	remove_task(id);
}

/*Server not processed angles. Always empty.*/
public fw_PlaybackEvent( iFlags, id, eventIndex )
{
	if(id > 0 && id < 33 && g_iDemoHelperInitStage[id] == -1 && iFlags == 1)
	{
		if (floatabs(g_flGameTimeReal - g_flLastEventTime[id]) > 1.0)
		{
			g_flLastEventTime[id] = g_flGameTimeReal;
			WriteDemoInfo(id, "UDS/XEVENT/%i", eventIndex);
		}
	}
	
	return FMRES_IGNORED;
}

/* JUMP DETECTION FROM ENGINE */
public HC_CBasePlayer_Jump_Pre(id) 
{
	new iFlags = get_entvar(id,var_flags);
	
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
	
	if (get_entvar(id, var_oldbuttons) & IN_JUMP || get_entvar(id, var_button) & IN_JUMP)
		WriteDemoInfo(id, "UDS/JMP/2/%i", g_iJumpCount[id]);
	else 
		WriteDemoInfo(id, "UDS/JMP/1/%i", g_iJumpCount[id]);

	g_iJumpCount[id]++;

	return HC_CONTINUE;
}

public PM_Move(const id)
{
	if (!is_user_hltv(id) && !is_user_bot(id))
	{
		new button = get_entvar(id, var_button);
		new cmdx = get_pmove(pm_cmd);
		new Float:curtime = g_flGameTimeReal;

		static Float:tmpAngles1[3];
		static Float:tmpAngles2[3];
		
		if (g_flStartCmdTime[id] > 0.0)
		{
			get_pmove(pm_oldangles, tmpAngles1);
			get_pmove(pm_angles, tmpAngles2);
			WriteDemoInfo(id, "UDS/ACMD/%i/%i/%i/%f/%f/%f/%f/%f/%f/%f", get_ucmd(cmdx, ucmd_lerp_msec), get_ucmd(cmdx, ucmd_msec),g_iFrameNum[id],tmpAngles1[0], tmpAngles1[1], 
						tmpAngles2[0], tmpAngles2[1],g_flPMovePrevPrevAngles[id][0],g_flPMovePrevPrevAngles[id][1], curtime - g_flStartCmdTime[id]);
			g_flStartCmdTime[id] = -1.0;
		}
		
		if (g_flDelay1Msec[id] > 0.0 && (button & IN_ATTACK) && floatabs(curtime - g_flLastSendTime[id]) > 1.0)
		{
			get_pmove(pm_oldangles, tmpAngles1);
			get_pmove(pm_angles, tmpAngles2);
			WriteDemoInfo(id, "UDS/SCMD/%i/%i/%i/%f/%f/%f/%f/%f/%f/%f/%f", get_ucmd(cmdx, ucmd_lerp_msec), get_ucmd(cmdx, ucmd_msec),g_iFrameNum[id],tmpAngles1[0], tmpAngles1[1], 
							tmpAngles2[0], tmpAngles2[1],g_flPMovePrevPrevAngles[id][0],g_flPMovePrevPrevAngles[id][1], curtime - g_flDelay2Time[id], g_flDelay1Msec[id]);
			g_iFrameNum[id]++;
			g_flStartCmdTime[id] = curtime;
	#if defined SMALL_TRAFFIC
			g_flLastSendTime[id] = curtime;
	#endif
			g_flDelay1Msec[id] = -1.0;
			
			get_pmove(pm_oldangles, g_flPMovePrevPrevAngles[id]);
		}
		
		if (!(button & IN_ATTACK))
		{
			g_flDelay1Msec[id] = curtime - g_flPMoveTime[id];
			g_flDelay2Time[id] = curtime;
			get_pmove(pm_oldangles, g_flPMovePrevPrevAngles[id]);
		}
		
		//server_print("[%i] %f = %i [PM gametime:] %f [PM frametime:] %f [MSEC:] %i = %i", id, curtime - g_flPMoveTime[id],button,g_flGameTimeReal,get_pmove(pm_frametime), get_ucmd(cmdx, ucmd_lerp_msec), get_ucmd(cmdx, ucmd_msec));
		
		g_flPMoveTime[id] = curtime;
	}
	return HC_CONTINUE;
}

public UnrealDemoHelpInitialize(const cmd[], source, id)
{
	if (id > 0 && id <= MaxClients)
	{
		new fullcmd[256];
		read_argv(0, fullcmd, charsmax(fullcmd));
		if (equal(fullcmd,"fullupdate"))
		{
			if (task_exists(id))
			{
				g_bNeedResend[id] = true;
			}
			else 
			{
				g_flLastEventTime[id] = 0.0;
				g_flLastSendTime[id] = 0.0;
				g_iFrameNum[id] = 0;
				g_iDemoHelperInitStage[id] = 0;

				remove_task(id);
				if (!is_user_hltv(id) && !is_user_bot(id))
				{
					set_task(1.25,"DemoHelperInitializeTask",id);
				}
			}
		}
	}
}

public DemoHelperInitializeTask(id)
{	
	if (!is_user_connected(id))
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
			WriteDemoInfo(id,"UDS/MINR/%i",get_cvar_num("sv_minrate"));
			WriteDemoInfo(id,"UDS/MAXR/%i",get_cvar_num("sv_maxrate"));
			WriteDemoInfo(id,"UDS/MINUR/%i",get_cvar_num("sv_minupdaterate"));
			WriteDemoInfo(id,"UDS/MAXUR/%i",get_cvar_num("sv_maxupdaterate"));
			set_task(1.0,"DemoHelperInitializeTask",id);
		}
		case 4:
		{
			if (g_bNeedResend[id])
			{
				g_bNeedResend[id] = false;
				g_iDemoHelperInitStage[id] = 0;
				set_task(1.0,"DemoHelperInitializeTask",id);
			}
			else 
			{
				g_flLastEventTime[id] = 0.0;
				g_iDemoHelperInitStage[id] = -1;
			}
		}
	}
}

// SVC_RESOURCELOCATION ignore all strings not started with http or https
// and can be used to save any info to demo
// Updated to more stable
public WriteDemoInfo(const index, const message[], any:... )
{
	static buffer[256];
	buffer[0] = EOS;

	message_begin(MSG_ONE, SVC_RESOURCELOCATION, _, index);
	new numArguments = numargs();
	if (numArguments == 2)
	{
		formatex(buffer, charsmax(buffer), "%s", message);
	}
	else 
	{
		vformat(buffer, charsmax(buffer), message, 3);
	}
	write_string(buffer);
	message_end();
}