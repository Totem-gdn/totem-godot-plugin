using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TotemEntities;
using TotemEntities.DNA;
using TotemServices;
using TotemServices.DNA;
public partial class MainScript : Node
{
	private TotemCore totemCore;
	private TotemUser user;
	Button AuthButton;
	Button SetButton;
	Button CompareButton;
	TextEdit inputLegacyNumber;
	TextEdit dataToCompoareInput;
	Label outLabel;
	Label authLabel;

	/// <summary>
	/// Id of your game, used for legacy records identification. 
	/// Note, that if you are targeting mobile platforms you also have to use this id for deepLink generation in
	/// *Window > Totem Generator > Generate Deep Link* menu
	/// </summary>
	public string _gameId = "TotemDemo";

	//private GameObject loginButton;
	/*
	private GameObject googleLoginObject;
	private GameObject profileNameObject;
	private TextMeshProUGUI profileNameText;

	private TMP_InputField legacyGameIdInput;
	private TMP_InputField dataToCompoareInput;
	private UIAssetsList assetList;
	private UIAssetLegacyRecordsList legacyRecordsList;
	private Animator popupAnimator;
	*/
	private string legacyGameIdInput = "0xCdd2C42dD75aB4Ad4d2f511564D9F421d98f7B5c";
	private List<TotemDNADefaultAvatar> assetList;
	private List<TotemLegacyRecord> legacyRecordsList;
	//Meta Data
	private TotemUser _currentUser;
	private List<TotemDNADefaultAvatar> _userAvatars;

	//Default Avatar reference - use for your game
	private TotemDNADefaultAvatar firstAvatar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		totemCore = new TotemCore(/*_gameId*/legacyGameIdInput);
		AddChild(totemCore.TotemNode);
		assetList = new List<TotemDNADefaultAvatar>();
		legacyRecordsList = new List<TotemLegacyRecord>();
		_userAvatars = new List<TotemDNADefaultAvatar>();
		AuthButton = GetNode<Button>("AuthButton");
		SetButton = GetNode<Button>("SetButton");
		CompareButton = GetNode<Button>("CompareButton");
		AuthButton.Pressed += AuthClicked;
		SetButton.Pressed += OnSetRecord;
		CompareButton.Pressed += OnGetRecords;
		inputLegacyNumber = GetNode<TextEdit>("SetInput");
		dataToCompoareInput = GetNode<TextEdit>("CompareInput");
		outLabel = GetNode<Label>("outLabel");
		authLabel = GetNode<Label>("AuthLabel");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	/// <summary>
	/// Initializing TotemCore
	/// </summary>
	void Start()
	{
		totemCore = new TotemCore(_gameId);

		//legacyGameIdInput.onEndEdit.AddListener(OnGameIdInputEndEdit);

		//UILoadingScreen.Instance.Show();
		//totemCore.AuthenticateLastUser(OnUserLoggedIn, (error) =>
		//{
		//    UILoadingScreen.Instance.Hide();
		//});
	}

	#region USER AUTHENTICATION
	private void AuthClicked()
	{
		//GD.Print("button_clicked");
		//UILoadingScreen.Instance.Show();

		//Login user
		totemCore.AuthenticateCurrentUser(OnUserLoggedIn);
		//GD.Print("button Event end");
	}

	private void OnUserLoggedIn(TotemUser user)
	{
		//GD.Print("OnUserLoggedIn");
		//Check if login was canceled
		if (user == null)
		{
			//UILoadingScreen.Instance.Hide();
			GD.Print("Login canceled");
			return;
		}
		GD.Print("Login complete");
		this.user = user;
		authLabel.Text = user.Name;
		GD.Print("userName: " + this.user.Name);
		GD.Print("user Email: " + this.user.Email);
		GD.Print("user public key: " + this.user.PublicKey);
		GD.Print("user ProfileImageUrl: " + this.user.ProfileImageUrl);
		//Using default filter with a default avatar model. You can implement your own filters and/or models
		totemCore.GetUserAvatars<TotemDNADefaultAvatar>(user, TotemDNAFilter.DefaultAvatarFilter, (avatars) =>
		{
			GD.Print("Avatars:");
			foreach(var avatar in avatars)
			{
				GD.Print("\t" + avatar.ToString());
			}
			//googleLoginObject.SetActive(false);
			//profileNameObject.SetActive(true);
			//profileNameText.SetText(user.Name);
			//UI
			assetList.Clear();
			legacyRecordsList.Clear();

			//Avatars
			_userAvatars = avatars;
			firstAvatar = avatars.Count > 0 ? avatars[0] : null;
			//
			//UI Example Methods
			BuildAvatarList();
			ShowAvatarRecords();
		});
		//GD.Print("After GetUserAvatars");
		totemCore.GetUserItems<TotemDNADefaultItem>(user, TotemDNAFilter.DefaultItemFilter, (items) =>
		{
			GD.Print("Items:");
			foreach (var item in items)
			{
				GD.Print("\t" + item.ToString());
			}
		});
		//GD.Print("After GetUserItems");

		SetButton.Disabled = false;
		CompareButton.Disabled = false;
	}


	public void ShowAvatarRecords()
	{
		if (firstAvatar == null)
		{
			return;
		}


		GetLegacyRecords(firstAvatar, TotemAssetType.avatar, (records) =>
		{
			outLabel.Text = string.Empty;
			//UIAssetLegacyRecordsList.Instance.BuildList(firstAvatar, records);
			GD.Print($"{records.Count} records");
			GD.Print("firstAvatar - " + firstAvatar + "\nrecords: \n");
			foreach(var item in records)
			{
				legacyRecordsList.Add(item);
				GD.Print("\t" + item.ToString());
				outLabel.Text += item.ToString() + "\n";

			}
			//UILoadingScreen.Instance.Hide();
		});
	}
	#endregion

	#region LEGACY RECORDS
	/// <summary>
	/// Add a new Legacy Record to a specific Totem Asset.
	/// </summary>
	public void AddLegacyRecord(object asset, TotemAssetType assetType, int data)
	{
		//UILoadingScreen.Instance.Show();
		totemCore.AddLegacyRecord(asset, assetType, data.ToString(), (record) =>
		{
			legacyRecordsList.Add(record);
			GD.Print(record.ToString());
			//UILoadingScreen.Instance.Hide();
			//popupAnimator.Play("Write Legacy");
		});
	}

	/// <summary>
	/// Add a new Legacy Record to the first Totem Avatar.
	/// </summary>
	public void AddLegacyToFirstAvatar(int data)
	{
		AddLegacyRecord(firstAvatar, TotemAssetType.avatar, data);
	}

	public void GetLegacyRecords(object asset, TotemAssetType assetType, UnityAction<List<TotemLegacyRecord>> onSuccess)
	{
		totemCore.GetLegacyRecords(asset, assetType, onSuccess, string.IsNullOrEmpty(legacyGameIdInput/*.text*/) ? _gameId : legacyGameIdInput/*.text*/);
	}

	public void GetLastLegacyRecord(UnityAction<TotemLegacyRecord> onSuccess)
	{
		GetLegacyRecords(firstAvatar, TotemAssetType.avatar, (records) => { onSuccess.Invoke(records[records.Count - 1]); });
	}
	public void OnGetRecords()
	{
		if(dataToCompoareInput.Text.Length < legacyGameIdInput.Length)
		{
			dataToCompoareInput.Text = string.Empty;
			dataToCompoareInput.PlaceholderText = "Error\nGame Id";
			//return;
		}
		GetLegacyRecords(firstAvatar, TotemAssetType.avatar, (records) =>
		{
			outLabel.Text = "";
			GD.Print("Records:");
			foreach(var record in records)
			{
				GD.Print("\t" + record);
				outLabel.Text += record + "\n";
			}
		});
	}

	public void CompareLastLegacyRecord()
	{
		GetLastLegacyRecord((record) =>
		{
			string valueToCheckText = dataToCompoareInput.Text;
			if (valueToCheckText.Equals(record.data))
			{
				//popupAnimator.Play("Read Legacy");
				//GD.Print("popupAnimator.Play(\"Read Legacy\");");
				GD.Print(record.data);
			}
			else
			{
				GD.PrintErr("Not Compare");
			}
		}
		);
	}
	public void OnSetRecord()
	{
		int customInt = 0;
		if (int.TryParse(inputLegacyNumber.Text, out customInt))
		{
			AddLegacyRecord(firstAvatar, TotemAssetType.avatar, customInt);
		}
	}
	#endregion

	#region UI EXAMPLE METHOD

	private void BuildAvatarList()
	{
		//assetList.BuildList(_userAvatars);
		assetList = new List<TotemDNADefaultAvatar>(_userAvatars);
	}

	private void OnGameIdInputEndEdit(string text)
	{
		ShowAvatarRecords();
	}

	#endregion
}
public class NYJamItem : TotemDNADefaultItem
{
	public string origin;
}


