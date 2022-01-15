#include <amxmodx>

#define PLUGIN "Unreal Demo Plugin"
#define AUTHOR "karaulov"
#define VERSION "1.0"

public plugin_init() 
{
	register_plugin(PLUGIN, VERSION, AUTHOR);
	register_clcmd("fullupdate", "UnrealDemoHelpInitialize");
}


public UnrealDemoHelpInitialize(id) 
{
	new szAuth[64];
	get_user_authid(id,szAuth,charsmax(szAuth));
	WriteDemoInfo(id,"UDS/AUTH/%s",szAuth);
	
	new szDate[64];
	get_time( "%Y.%m.%d %H:%M:%S", szDate, charsmax( szDate ) );
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