using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout (LayoutKind.Sequential, Pack=2)]
struct ConnexionDeviceState {
	// header
	public UInt16		version;	// kConnexionDeviceStateVers
	public UInt16		client;		// identifier of the target client when sending a state message to all user clients
	// command
	public UInt16		command;	// command for the user-space client
	public Int16		param;		// optional parameter for the specified command
	public Int32		value;		// optional value for the specified command
	public UInt64		time;		// timestamp for this message (clock_get_uptime)
	// raw report
	
	//[MarshalAs (UnmanagedType.ByValArray, SizeConst=8)]
	//public byte[]		report;		// raw USB report from the device
	public byte 		report0, report1, report2, report3, report4, report5, report6, report7;
	
	// processed data
	public UInt16		buttons8;	// buttons (first 8 buttons only, for backwards binary compatibility- use "buttons" field instead)
	
	// [MarshalAs (UnmanagedType.ByValArray, SizeConst=6)]
	// public Int16[]	axis;		// x, y, z, rx, ry, rz
	public Int16 		x, y, z, rx, ry, rz;
	
	// reserved for future use
	public UInt16		address;	// USB device address, used to tell one device from the other
	public UInt32		buttons;	// buttons
};


class ConnexionClient {
	// Constants defined in ConnexionClient.h
	
	#region - Constants -
	public const ushort kConnexionClientModeTakeOver = 1;
	
	//==============================================================================
	// Client commands
	
	// The following assignments must be executed by the client:
	
	public const int 	kConnexionCmdNone					= 0;
	public const int 	kConnexionCmdHandleRawData			= 1;
	public const int 	kConnexionCmdHandleButtons			= 2;
	public const int 	kConnexionCmdHandleAxis				= 3;
	public const int 	kConnexionCmdAppSpecific			= 10;
	
	
	// The following messages are sent from the Kernel driver to user space clients:
	
	// #define kConnexionMsgDeviceState		'3dSR'		// forwarded device state data
	public const UInt32 kConnexionMsgDeviceState = 0x33645352;
	
	// #define kConnexionMsgCalibrateDevice	'3dSC'		// device state data to be used for calibration
	public const UInt32 kConnexionMsgCalibrateDevice = 0x33645343;
	
	// #define kConnexionMsgPrefsChanged		'3dPC'		// notify clients that the current app prefs have changed
	public const UInt32 kConnexionMsgPrefsChanged = 0x33645043;
	
	// #define kConnexionMsgDoMapping			'3dDA'		// execute a mapping through the user space helper (should be ignored by clients)
	public const UInt32 kConnexionMsgDoMapping = 0x33644441;
	
	// #define kConnexionMsgDoMappingDown		'3dMD'		// down event for mappings that require both down and up events (should be ignored by clients)
	public const UInt32 kConnexionMsgDoMappingDown = 0x33644D44;
	
	// #define kConnexionMsgDoMappingUp		'3dMU'		// up event for mappings that require both down and up events (should be ignored by clients)
	public const UInt32 kConnexionMsgDoMappingUp = 0x33644D55;
	
	// #define kConnexionMsgDoLongPress		'3dLP'		// forward long press events to the user space helper (should be ignored by clients)
	public const UInt32 kConnexionMsgDoLongPress = 0x33644C50;
	
	// #define kConnexionMsgBatteryStatus		'3dBS'		// forward battery status info to the user space helper (should be ignored by clients)
	public const UInt32 kConnexionMsgBatteryStatus = 0x33644253;
	
	
	// Client capability mask constants (this mask defines which buttons and controls should be sent to clients, the others are handled by the driver)
	
	public const uint kConnexionMaskButton1 = 0x0001;
	public const uint kConnexionMaskButton2 = 0x0002;
	public const uint kConnexionMaskButton3 = 0x0004;
	public const uint kConnexionMaskButton4 = 0x0008;
	public const uint kConnexionMaskButton5 = 0x0010;
	public const uint kConnexionMaskButton6 = 0x0020;
	public const uint kConnexionMaskButton7 = 0x0040;
	public const uint kConnexionMaskButton8 = 0x0080;
	
	public const uint kConnexionMaskAxis1 = 0x0100;
	public const uint kConnexionMaskAxis2 = 0x0200;
	public const uint kConnexionMaskAxis3 = 0x0400;
	public const uint kConnexionMaskAxis4 = 0x0800;
	public const uint kConnexionMaskAxis5 = 0x1000;
	public const uint kConnexionMaskAxis6 = 0x2000;
	
	public const uint kConnexionMaskButtons 	= 0x00FF;		// note: this only specifies the first 8 buttons, kept for backwards compatibility
	public const uint kConnexionMaskAxisTrans 	= 0x0700;
	public const uint kConnexionMaskAxisRot 	= 0x3800;
	public const uint kConnexionMaskAxis 		= 0x3F00;
	public const uint kConnexionMaskAll 		= 0x3FFF;
	
	// Added in version 10:0 to support all 32 buttons on the SpacePilot Pro, use with the new SetConnexionClientButtonMask API
	
	public const uint kConnexionMaskButton9 	= 0x00000100;
	public const uint kConnexionMaskButton10 	= 0x00000200;
	public const uint kConnexionMaskButton11 	= 0x00000400;
	public const uint kConnexionMaskButton12 	= 0x00000800;
	public const uint kConnexionMaskButton13 	= 0x00001000;
	public const uint kConnexionMaskButton14 	= 0x00002000;
	public const uint kConnexionMaskButton15 	= 0x00004000;
	public const uint kConnexionMaskButton16 	= 0x00008000;
	
	public const uint kConnexionMaskButton17 	= 0x00010000;
	public const uint kConnexionMaskButton18 	= 0x00020000;
	public const uint kConnexionMaskButton19 	= 0x00040000;
	public const uint kConnexionMaskButton20 	= 0x00080000;
	public const uint kConnexionMaskButton21 	= 0x00100000;
	public const uint kConnexionMaskButton22 	= 0x00200000;
	public const uint kConnexionMaskButton23 	= 0x00400000;
	public const uint kConnexionMaskButton24 	= 0x00800000;
	
	public const uint kConnexionMaskButton25 	= 0x01000000;
	public const uint kConnexionMaskButton26 	= 0x02000000;
	public const uint kConnexionMaskButton27 	= 0x04000000;
	public const uint kConnexionMaskButton28 	= 0x08000000;
	public const uint kConnexionMaskButton29 	= 0x10000000;
	public const uint kConnexionMaskButton30 	= 0x20000000;
	public const uint kConnexionMaskButton31 	= 0x40000000;
	public const uint kConnexionMaskButton32 	= 0x80000000;
	
	public const uint kConnexionMaskAllButtons 	= 0xFFFFFFFF;
	
	// Masks for client-controlled feature switches
	
	public const uint kConnexionSwitchZoomOnY 			= 0x0001;
	public const uint kConnexionSwitchDominant 			= 0x0002;
	public const uint kConnexionSwitchEnableAxis1 		= 0x0004;
	public const uint kConnexionSwitchEnableAxis2 		= 0x0008;
	public const uint kConnexionSwitchEnableAxis3 		= 0x0010;
	public const uint kConnexionSwitchEnableAxis4 		= 0x0020;
	public const uint kConnexionSwitchEnableAxis5 		= 0x0040;
	public const uint kConnexionSwitchEnableAxis6 		= 0x0080;
	public const uint kConnexionSwitchReverseAxis1 		= 0x0100;
	public const uint kConnexionSwitchReverseAxis2 		= 0x0200;
	public const uint kConnexionSwitchReverseAxis3 		= 0x0400;
	public const uint kConnexionSwitchReverseAxis4 		= 0x0800;
	public const uint kConnexionSwitchReverseAxis5 		= 0x1000;
	public const uint kConnexionSwitchReverseAxis6 		= 0x2000;
	
	public const uint kConnexionSwitchEnableTrans 		= 0x001C;
	public const uint kConnexionSwitchEnableRot 		= 0x00E0;
	public const uint kConnexionSwitchEnableAll 		= 0x00FC;
	public const uint kConnexionSwitchReverseTrans 		= 0x0700;
	public const uint kConnexionSwitchReverseRot 		= 0x3800;
	public const uint kConnexionSwitchReverseAll 		= 0x3F00;
	
	public const uint kConnexionSwitchesDisabled 		= 0x80000000;	// use driver defaults instead of client-controlled switches
	
	#endregion - Constants -
	
	
	#region - Callbacks -
	//==============================================================================
	// Callback procedure types
	
	//typedef void	(*ConnexionAddedHandlerProc)		(io_connect_t connection);
	public delegate void ConnexionAddedHandlerProc(UInt32 connection);
	
	//typedef void	(*ConnexionRemovedHandlerProc)		(io_connect_t connection);
	public delegate void ConnexionRemovedHandlerProc(UInt32 connection);
	
	//typedef void	(*ConnexionMessageHandlerProc)		(io_connect_t connection, natural_t messageType, void *messageArgument);
	public delegate void ConnexionMessageHandlerProc(UInt32 connection, UInt32 messageType, IntPtr messageArgument);
	
	
	// NOTE for ConnexionMessageHandlerProc:
	// when messageType == kConnexionMsgDeviceState, messageArgument points to ConnexionDeviceState with size kConnexionDeviceStateSize
	// when messageType == kConnexionMsgPrefsChanged, messageArgument points to the target application signature with size sizeof(UInt32)
	#endregion - Callbacks -
	
	#region - 3DconnexionClient API -
	//==============================================================================
	// Public APIs to be called once when the application starts up or shuts down

	private const string DllName = "3DconnexionClient.framework/3DconnexionClient";

	//OSErr			InstallConnexionHandlers			(ConnexionMessageHandlerProc messageHandler, ConnexionAddedHandlerProc addedHandler, ConnexionRemovedHandlerProc removedHandler);
	[DllImport (DllName)]
	public static extern void InstallConnexionHandlers (ConnexionMessageHandlerProc messageHandler, ConnexionAddedHandlerProc addedHandler, ConnexionRemovedHandlerProc removedHandler);
	
	//void			CleanupConnexionHandlers			(void);
	[DllImport (DllName)]
	public static extern void CleanupConnexionHandlers ();
	
	//==============================================================================
	// Public APIs to be called whenever the app wants to start/stop receiving data
	// the mask parameter (client capabilities mask) specifies which controls must be forwarded to the client
	// buttonMask (previously part of the client capabilities mask) specifies which buttons must be forwarded to the client
	
	//UInt16			RegisterConnexionClient				(UInt32 signature, UInt8 *name, UInt16 mode, UInt32 mask);
	[DllImport (DllName)]
	public static extern ushort RegisterConnexionClient (uint signature, string name, ushort mode, uint mask);
	
	//void			SetConnexionClientMask				(UInt16 clientID, UInt32 mask);
	[DllImport (DllName)]
	public static extern void SetConnexionClientMask (ushort clientID, uint mask);
	
	//void			SetConnexionClientButtonMask		(UInt16 clientID, UInt32 buttonMask);
	[DllImport (DllName)]
	public static extern void SetConnexionClientButtonMask (ushort clientID, uint buttonMask);
	
	//void			UnregisterConnexionClient			(UInt16 clientID);
	[DllImport (DllName)]
	public static extern void UnregisterConnexionClient (ushort clientID);
	
	//==============================================================================
	// Public API to send control commands to the driver and retrieve a result value
	// Note: the new ConnexionClientControl variant is strictly required for
	// kConnexionCtlSetSwitches and kConnexionCtlClearSwitches but also works for
	// all other Control calls. The old variant remains for backwards compatibility.
	
	//OSErr			ConnexionControl					(UInt32 message, SInt32 param, SInt32 *result);
	//OSErr			ConnexionClientControl				(UInt16 clientID, UInt32 message, SInt32 param, SInt32 *result);
	
	//==============================================================================
	// Public API to fetch the current device preferences for either the first connected device or a specific device type (kDevID_Xxx)
	
	//OSErr			ConnexionGetCurrentDevicePrefs		(UInt32 deviceID, ConnexionDevicePrefs *prefs);
	
	//==============================================================================
	// Public API to set all button labels in the iOS/Android "virtual device" apps
	
	//OSErr			ConnexionSetButtonLabels			(UInt8 *labels, UInt16 size);
	
	// Labels data is a series of 32 variable-length null-terminated UTF8-encoded strings.
	// The sequence of strings follows the SpacePilot Pro button numbering.
	// Empty strings revert the button label to its default value.
	// As an example, this data would set the label for button Top to "Top" and
	// revert all other button labels to their default values:
	// 
	//	0x00,						// empty string for Menu
	//	0x00,						// empty string for Fit
	//	0x54, 0x6F, 0x70, 0x00,		// utf-8 encoded "Top" string for Top
	//	0x00,						// empty string for Left
	//	0x00, 0x00, 0x00, 0x00,		// empty strings for Right, Front, etc...
	//	0x00, 0x00, 0x00, 0x00,
	//	0x00, 0x00, 0x00, 0x00,
	//	0x00, 0x00, 0x00, 0x00,
	//	0x00, 0x00, 0x00, 0x00,
	//	0x00, 0x00, 0x00, 0x00,
	//	0x00, 0x00, 0x00, 0x00
	//==============================================================================
	#endregion - 3DconnexionClient API -


}

