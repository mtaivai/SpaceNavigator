using System;
using System.Runtime.InteropServices;
using TDx.TDxInput;
using UnityEngine;


class SpaceNavigatorOSX : SpaceNavigator {	


	private ushort clientID = 0;
	private bool connected = false;


	// Latest translation values from the device
	private float x, y, z;

	// Latest rotation values from the device
	private float rx, ry, rz;

	private const float TransSensScale = 0.005f, RotSensScale = 0.015f;

	private bool Connected {
		get { return clientID != 0 && connected; }
	}
	private void SetTranslation(Int16 x, Int16 y, Int16 z) {
		this.x = (float)x;
		this.y = (float)y;
		this.z = (float)z;
	}
	private void SetRotation(Int16 rx, Int16 ry, Int16 rz) {
		this.rx = (float)rx;
		this.ry = (float)ry;
		this.rz = (float)rz;
	}
	private void ResetTranslation() {
		SetTranslation(0, 0, 0);
	}
	private void ResetRotation() {
		SetRotation(0, 0, 0);
	}
	private void ResetAll() {
		ResetTranslation();
		ResetRotation();
	}

	private void ConnexionAdded() {
		SubInstance.ResetAll ();
	}
	
	private void ConnexionRemoved() {
		SubInstance.ResetAll ();
	}

	private void ConnexionDeviceStateChanged(ref ConnexionDeviceState ds) {
		switch (ds.command) {
		case ConnexionClient.kConnexionCmdHandleAxis:
			SetTranslation(ds.x, ds.y, ds.z);
			SetRotation(ds.rx, ds.ry, ds.rz);
			break;
		case ConnexionClient.kConnexionCmdHandleButtons:
		case ConnexionClient.kConnexionCmdAppSpecific:
		case ConnexionClient.kConnexionCmdHandleRawData:
			break;
		}
				
	}

	// Public API
	public override Vector3 GetTranslation() {
		float sensitivity = Application.isPlaying ? PlayTransSens : TransSens[CurrentGear];
		if (Connected && !LockTranslationAll) {
			//Debug.Log ("Translation: " + new Vector3(x, y, z));
			return new Vector3(
				LockTranslationX ? 0 : x,
				LockTranslationY ? 0 : -z,
				LockTranslationZ ? 0 : -y) *
				sensitivity * TransSensScale;
		} else {
			return Vector3.zero;
		}
	}
	public override Quaternion GetRotation() {
		float sensitivity = Application.isPlaying ? PlayRotSens : RotSens;
		if (Connected && !LockRotationAll) {
			Vector3 v = new Vector3(
				LockRotationX ? 0 : -rx, 
				LockRotationY ? 0 : rz, 
				LockRotationZ ? 0 : ry) * sensitivity * RotSensScale;
			//Debug.Log ("Rotation: " + new Vector3(rx, ry, rz) + "; v: " + v);
			return Quaternion.Euler(v);
		} else {
			return Quaternion.identity;
		}
		
	}


#region - ConnexionClient Implementation -
	private void InitConnexionClient() {
		string appName = "Unity";
		appName = (char)appName.Length + appName;
		
		clientID = ConnexionClient.RegisterConnexionClient(
			(ushort)0, appName, ConnexionClient.kConnexionClientModeTakeOver, ConnexionClient.kConnexionMaskAll);
		Debug.Log ("Registered connection client: " + clientID + " with app name: " + appName);
		
		// Register connection handler
		ConnexionClient.InstallConnexionHandlers(
			ConnexionMessageHandlerProcImpl, 
			ConnexionAddedHandlerProcImpl, 
			ConnexionRemovedHandlerProcImpl);
		
		connected = true;
	}

	private void DisconnectConnexionClient() {
		if (connected) {
			try {
				ConnexionClient.CleanupConnexionHandlers();
				connected = false;
			} catch (Exception e) {
				Debug.LogError("Failed to clean up connection handlers: " + e.ToString());
			}
		}
		
		if (clientID != 0) {
			try {
				ConnexionClient.UnregisterConnexionClient(clientID);
				Debug.Log ("Unregistered connection client: " + clientID);
				clientID = 0;
			}
			catch (Exception ex) {
				Debug.LogError("Failed to unregister the connection client: " + ex.ToString());
			}
		}
	}

	private static void ConnexionAddedHandlerProcImpl(UInt32 connection) {
		SubInstance.ConnexionAdded();
	}
	
	private static void ConnexionRemovedHandlerProcImpl(UInt32 connection) {
		SubInstance.ConnexionRemoved();
	}
	
	private static void ConnexionMessageHandlerProcImpl(UInt32 connection, UInt32 messageType, IntPtr messageArgument) {
		//Debug.Log("Connexion Message: " + connection + "; messageType=" + messageType.ToString("X"));
		
		switch (messageType) {
		case ConnexionClient.kConnexionMsgDeviceState:
			try {
				ConnexionDeviceState ds = (ConnexionDeviceState)Marshal.PtrToStructure(messageArgument, typeof(ConnexionDeviceState));
				SubInstance.ConnexionDeviceStateChanged(ref ds);
			} catch (Exception e) {
				Debug.LogError(e);
			}
			
			break;
		case ConnexionClient.kConnexionMsgPrefsChanged:
			// TODO NOT IMPLEMENTED
			break;
		default:
			// Other messages should be ignored by the client
			break;
		}
		
	}
#endregion - ConnexionClient Implementation -

	
	#region - Singleton -
	/// <summary>
	/// Private constructor, prevents a default instance of the <see cref="SpaceNavigatorWindows" /> class from being created.
	/// </summary>
	private SpaceNavigatorOSX() {
		ResetAll();
		try {
			InitConnexionClient();
		}
		catch (Exception ex) {

			Debug.LogError(ex.ToString());
		}
		//client.deviceStateHandler += new EventHandler(ConnexionDeviceStateHandler);
		//client.DeviceStateChanged += DeviceStateChanged;
		//client.DeviceStateChanged += (object sender, EventArgs e) => { Debug.Log(e); };
	}



	public static SpaceNavigatorOSX SubInstance {
		get { return _subInstance ?? (_subInstance = new SpaceNavigatorOSX()); }
	}
	private static SpaceNavigatorOSX _subInstance;
	#endregion - Singleton -

	#region - IDisposable -
	public override void Dispose() {
		ResetAll();
		_subInstance = null;

		if (clientID != 0) {
			DisconnectConnexionClient();
		}
	}
	#endregion - IDisposable -
}
