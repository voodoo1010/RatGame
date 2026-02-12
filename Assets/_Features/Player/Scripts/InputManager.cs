using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    public static InputManager Instance
    {
        get { return instance; }
    }

    public enum Bindings
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Sprint,
        Interact,
        Reload,
        Shoot,
        Aim,
        Inventory,
        Knife,
        Revolver,
        Shotgun,
        Rifle,
        Torch,
        Back
    }

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        inputActions = new InputSystem_Actions();
        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }
        // inputActions.Player.Torch.performed += ctx => PlayerWeapons.Instance.ToggleTorch();
        // inputActions.Player.Escape.performed += ctx =>
        // {

        //     if (
        //        ContainerSearchingUI.Instance.IsOpen()
        //     || ConfirmItemUseUI.Instance.IsOpen()
        //     || NoteContentUI.Instance.IsOpen()
        //     || InventoryManager.Instance.IsOpen()
        //     ) return;

        //     EscapeMenuUI.Instance.Toggle();
        // };
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }


    //helper functions
    public Vector2 GetPlayerMovement()
    {
        return inputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return inputActions.Player.Look.ReadValue<Vector2>();
    }

    public bool IsPlayerSprinting()
    {
        return inputActions.Player.Sprint.phase == UnityEngine.InputSystem.InputActionPhase.Performed;
    }

    public bool IsPlayerMoving()
    {
        return inputActions.Player.Move.IsInProgress();
    }

    public bool GetPlayerInteract()
    {
        return inputActions.Player.Interact.WasPressedThisFrame();
    }


    public bool GetPlayerShoot()
    {
        return inputActions.Player.Shoot.triggered;
    }

    public bool GetPlayerShootAuto()
    {
        return inputActions.Player.Shoot.IsInProgress();
    }

    public bool GetPlayerReload()
    {
        return inputActions.Player.Reload.triggered;
    }

    public bool GetQuickSlot1()
    {
        return inputActions.Player.QuickSlotOne.triggered;
    }
    public bool GetQuickSlot2()
    {
        return inputActions.Player.QuickSlotTwo.triggered;
    }
    public bool GetQuickSlot3()
    {
        return inputActions.Player.QuickSlotThree.triggered;
    }
    public bool GetQuickSlot4()
    {
        return inputActions.Player.QuickSlotFour.triggered;
    }

    public bool GetPlayerAim()
    {
        return inputActions.Player.Aim.IsInProgress();
    }

    public bool GetPlayerAimToggle()
    {
        return inputActions.Player.Aim.WasPressedThisFrame();
    }

    public bool GetInventoryOpen()
    {
        return inputActions.Player.Inventory.triggered;
    }

    public bool GetPlayerCrouch()
    {
        return inputActions.Player.Crouch.triggered;
    }

    public bool GetPlayerMelee()
    {
        return inputActions.Player.Melee.triggered;
    }

    public bool GetTorchToggle()
    {
        return inputActions.Player.Torch.triggered;
    }

    public bool GetPlayerEscape()
    {
        return inputActions.Player.Escape.triggered;
    }

    public bool GetPlayerTorch()
    {
        return inputActions.Player.Torch.triggered;
    }

    public bool GetNavigateRightTriggered()
    {
        return inputActions.UI.Navigate.WasPressedThisFrame() && inputActions.UI.Navigate.ReadValue<Vector2>().x > 0;
    }

    public bool GetNavigateLeftTriggered()
    {
        return inputActions.UI.Navigate.WasPressedThisFrame() && inputActions.UI.Navigate.ReadValue<Vector2>().x < 0;
    }

    public bool GetNavigateUpTriggered()
    {
        return inputActions.UI.Navigate.WasPressedThisFrame() && inputActions.UI.Navigate.ReadValue<Vector2>().y > 0;
    }

    public bool GetNavigateDownTriggered()
    {
        return inputActions.UI.Navigate.WasPressedThisFrame() && inputActions.UI.Navigate.ReadValue<Vector2>().y < 0;
    }

    public bool GetUseItem()
    {
        return inputActions.UI.UseItem.triggered;
    }

    public bool GetCloseTriggered()
    {
        return inputActions.UI.Close.triggered;
    }

    public bool GetUIBackTriggered()
    {
        return inputActions.UI.Back.triggered;
    }

    // public string GetInputString(KeyOption keyOption)
    // {
    //     switch (keyOption)
    //     {
    //         case KeyOption.NavigateUp:
    //             return GetStringOfSafeLength(inputActions.UI.Navigate.bindings[1].ToDisplayString().ToUpper());
    //         case KeyOption.NavigateDown:
    //             return GetStringOfSafeLength(inputActions.UI.Navigate.bindings[3].ToDisplayString().ToUpper());
    //         case KeyOption.NavigateLeft:
    //             return GetStringOfSafeLength(inputActions.UI.Navigate.bindings[5].ToDisplayString().ToUpper());
    //         case KeyOption.NavigateRight:
    //             return GetStringOfSafeLength(inputActions.UI.Navigate.bindings[7].ToDisplayString().ToUpper());
    //         case KeyOption.UseItem:
    //             return GetStringOfSafeLength(inputActions.UI.UseItem.bindings[0].ToDisplayString().ToUpper());
    //         case KeyOption.Close:
    //             return GetStringOfSafeLength(inputActions.UI.Close.bindings[0].ToDisplayString().ToUpper());
    //         case KeyOption.Inventory:
    //             return GetStringOfSafeLength(inputActions.Player.Inventory.bindings[0].ToDisplayString().ToUpper());
    //         case KeyOption.Back:
    //             return GetStringOfSafeLength(inputActions.UI.Back.bindings[0].ToDisplayString().ToUpper());
    //         default:
    //             return null;
    //     }
    // }

    public string GetBindingString(Bindings binding)
    {
        switch (binding)
        {
            default:
            case Bindings.Move_Up:
                return GetStringOfSafeLength(inputActions.Player.Move.bindings[2].ToDisplayString().ToUpper());
            case Bindings.Move_Down:
                return GetStringOfSafeLength(inputActions.Player.Move.bindings[3].ToDisplayString().ToUpper());
            case Bindings.Move_Left:
                return GetStringOfSafeLength(inputActions.Player.Move.bindings[4].ToDisplayString().ToUpper());
            case Bindings.Move_Right:
                return GetStringOfSafeLength(inputActions.Player.Move.bindings[5].ToDisplayString().ToUpper());
            case Bindings.Sprint:
                return GetStringOfSafeLength(inputActions.Player.Sprint.bindings[0].ToDisplayString().ToUpper().Replace("LEFT SHIFT", "SHIFT"), 5);
            case Bindings.Interact:
                return GetStringOfSafeLength(inputActions.Player.Interact.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Reload:
                return GetStringOfSafeLength(inputActions.Player.Reload.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Shoot:
                return GetStringOfSafeLength(inputActions.Player.Shoot.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Aim:
                return GetStringOfSafeLength(inputActions.Player.Aim.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Inventory:
                return GetStringOfSafeLength(inputActions.Player.Inventory.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Knife:
                return GetStringOfSafeLength(inputActions.Player.QuickSlotOne.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Revolver:
                return GetStringOfSafeLength(inputActions.Player.QuickSlotTwo.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Shotgun:
                return GetStringOfSafeLength(inputActions.Player.QuickSlotThree.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Rifle:
                return GetStringOfSafeLength(inputActions.Player.QuickSlotFour.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Torch:
                return GetStringOfSafeLength(inputActions.Player.Torch.bindings[0].ToDisplayString().ToUpper());
            case Bindings.Back:
                return GetStringOfSafeLength(inputActions.UI.Back.bindings[0].ToDisplayString().ToUpper());
        }
    }
    public void RebindKey(Bindings binding, Action OnRebindComplete)
    {
        inputActions.Player.Disable();
        InputAction action;
        int bindingIndex = 0;
        switch (binding)
        {
            default:
            case Bindings.Move_Up:
                action = inputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Bindings.Move_Down:
                action = inputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Bindings.Move_Left:
                action = inputActions.Player.Move;
                bindingIndex = 4;
                break;
            case Bindings.Move_Right:
                action = inputActions.Player.Move;
                bindingIndex = 5;
                break;
            case Bindings.Sprint:
                action = inputActions.Player.Sprint;
                bindingIndex = 0;
                break;
            case Bindings.Interact:
                action = inputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Bindings.Reload:
                action = inputActions.Player.Reload;
                bindingIndex = 0;
                break;
            case Bindings.Shoot:
                action = inputActions.Player.Shoot;
                bindingIndex = 0;
                break;
            case Bindings.Aim:
                action = inputActions.Player.Aim;
                bindingIndex = 0;
                break;
            case Bindings.Inventory:
                action = inputActions.Player.Inventory;
                bindingIndex = 0;
                break;
            case Bindings.Knife:
                action = inputActions.Player.QuickSlotOne;
                bindingIndex = 0;
                break;
            case Bindings.Revolver:
                action = inputActions.Player.QuickSlotTwo;
                bindingIndex = 0;
                break;
            case Bindings.Shotgun:
                action = inputActions.Player.QuickSlotThree;
                bindingIndex = 0;
                break;
            case Bindings.Rifle:
                action = inputActions.Player.QuickSlotFour;
                bindingIndex = 0;
                break;
            case Bindings.Torch:
                action = inputActions.Player.Torch;
                bindingIndex = 0;
                break;
            case Bindings.Back:
                action = inputActions.UI.Back;
                bindingIndex = 0;
                break;
        }

        action.PerformInteractiveRebinding(bindingIndex).OnComplete(callback =>
        {
            callback.Dispose();
            inputActions.Player.Enable();
            OnRebindComplete();
            PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, inputActions.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
        }).Start();
    }

    private string GetStringOfSafeLength(string input, int maxLength = 3)
    {
        if (input.Length > maxLength)
        {
            return input[..maxLength];
        }
        return input;
    }
}
