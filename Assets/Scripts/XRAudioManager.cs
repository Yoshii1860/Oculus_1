using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRAudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip _fallBackClip;

    [Header("Grab Interactables")]
    [SerializeField] private AudioSource _grabSource;
    [SerializeField] private AudioSource _activateSource;
    [Space(10)]
    [SerializeField] private AudioClip _grabClip;
    [SerializeField] private AudioClip _keyClip;
    [SerializeField] private AudioClip _activateGrabClip;
    [SerializeField] private AudioClip _activateWandClip;
    private XRGrabInteractable[] _grabInteractables;

    [Header("Drawer Interactables")]
    [SerializeField] private DrawerInteractable _drawer;
    [SerializeField] private XRSocketInteractor _drawerKeySocket;
    private AudioSource _drawerSocketSource;
    private AudioSource _drawerSource;
    private AudioClip _drawerSocketClip;
    private AudioClip _drawerMoveClip;
    
    [Header("Hinge Interactables")]
    [SerializeField] private SimpleHingeInteractable[] _cabinetDoors = new SimpleHingeInteractable[2];
    private AudioSource[] _cabinetDoorSources;
    private AudioClip _cabinetDoorMoveClip;

    [Header("Combo Lock Interactables")]
    [SerializeField] private CombinationLock _comboLock;
    private AudioSource _comboLockSource;
    private AudioClip _unlockClip;
    private AudioClip _comboButtonClip;
    private AudioClip _incorrectClip;

    [Header("Wall Interactables")]
    [SerializeField] private WallSystem _wall;
    [SerializeField] private XRSocketInteractor _wallSocket;
    private AudioSource _wallSource;
    private AudioSource _wallSocketSource;
    private AudioClip _wallExplosionClip;
    private AudioClip _wallSocketClip;

    private const string FALLBACKCLIP_NAME = "fallBackClip";

    private void OnEnable()
    {
        if (_fallBackClip == null)
        {
            _fallBackClip = AudioClip.Create(FALLBACKCLIP_NAME, 1, 1, 1000, true);
        }

        SetGrabInteractables();

        if (_drawer != null)
        {
            SetDrawerInteractable();
        }

        _cabinetDoorSources = new AudioSource[_cabinetDoors.Length];
        for (int i = 0; i < _cabinetDoors.Length; i++)
        {
            if (_cabinetDoors[i] != null)
            {
                SetCabinetDoors(i);
            }
        }

        if (_comboLock != null)
        {
            SetComboLock();
        }

        if (_wall != null)
        {
            SetWall();
        }
    }

    private void OnDisable()
    {
        if (_wall != null)
        {
            _wall.OnDestroy.RemoveListener(OnDestroyWall);
        }
    }

    private void OnSelectEnterGrab(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("Key"))
        {
            _grabSource.clip = _keyClip;
        }
        else
        {
            _grabSource.clip = _grabClip;
        }
        _grabSource.Play();
    }

    private void OnSelectExitGrab(SelectExitEventArgs args)
    {
        _grabSource.clip = _grabClip;
        _grabSource.Play();
    }

    private void OnActivatedGrab(ActivateEventArgs args)
    {
        GameObject tempGameObject = args.interactableObject.transform.gameObject;
        if (tempGameObject.GetComponent<WandControl>() != null)
        {
            _activateSource.clip = _activateWandClip;
        }
        else
        {
            _activateSource.clip = _activateGrabClip;
        }
        _activateSource.Play();
    }

    private void OnDestroyWall()
    {
        _wallSource.Play();
    }

    private void OnDrawerMove(SelectEnterEventArgs args)
    {
        _drawerSource.Play();
    }

    private void OnDrawerStop(SelectExitEventArgs args)
    {
        _drawerSource.Stop();
    }

    private void OnDrawerSocketed(SelectEnterEventArgs args)
    {
        _drawerSocketSource.Play();
    }

    private void OnWallSocketed(SelectEnterEventArgs args)
    {
        _wallSocketSource.Play();
    }

    private void OnCabinetDoorMove(SimpleHingeInteractable args)
    {
        for (int i = 0; i < _cabinetDoors.Length; i++)
        {
            if (args == _cabinetDoors[i])
            {
                _cabinetDoorSources[i].Play();
            }
        }
    }

    private void OnCabinetDoorStop(SelectExitEventArgs args)
    {
        for (int i = 0; i < _cabinetDoors.Length; i++)
        {
            if (args.interactableObject == _cabinetDoors[i])
            {
                _cabinetDoorSources[i].Stop();
            }
        }
    }

    private void OnComboUnlock()
    {
        _comboLockSource.clip = _unlockClip;
        _comboLockSource.Play();
    }

    private void OnIncorrectCombo()
    {
        _comboLockSource.clip = _incorrectClip;
        _comboLockSource.Play();
    }

    private void OnComboButtonPress()
    {
        _comboLockSource.clip = _comboButtonClip;
        _comboLockSource.Play();
    }

    private void CheckClip(ref AudioClip clip)
    {
        if (clip == null)
        {
            clip = _fallBackClip;
        }
    }

    private void SetGrabInteractables()
    {
        _grabInteractables = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);

        for (int i = 0; i < _grabInteractables.Length; i++)
        {
            _grabInteractables[i].selectEntered.AddListener(OnSelectEnterGrab);
            _grabInteractables[i].selectExited.AddListener(OnSelectExitGrab);
            _grabInteractables[i].activated.AddListener(OnActivatedGrab);
        }
    }

    private void SetDrawerInteractable()
    {
        _drawerSource = _drawer.transform.gameObject.AddComponent<AudioSource>();
        _drawerMoveClip = _drawer.GetDrawerMoveClip();
        CheckClip(ref _drawerMoveClip);
        _drawerSource.clip = _drawerMoveClip;
        _drawerSource.loop = true;

        _drawer.selectEntered.AddListener(OnDrawerMove);
        _drawer.selectExited.AddListener(OnDrawerStop);

        if (_drawerKeySocket != null)
        {
            _drawerSocketSource = _drawerKeySocket.gameObject.AddComponent<AudioSource>();
            _drawerSocketClip = _drawer.GetSocketedClip();
            CheckClip(ref _drawerSocketClip);
            _drawerSocketSource.clip = _drawerSocketClip;
            _drawerKeySocket.selectEntered.AddListener(OnDrawerSocketed);
        }
    }

    private void SetCabinetDoors(int index)
    {
        _cabinetDoorSources[index] = _cabinetDoors[index].gameObject.AddComponent<AudioSource>();
        _cabinetDoorMoveClip = _cabinetDoors[index].GetHingeMoveClip();
        CheckClip(ref _cabinetDoorMoveClip);
        _cabinetDoorSources[index].clip = _cabinetDoorMoveClip;
        _cabinetDoors[index].OnHingeSelected.AddListener(OnCabinetDoorMove);
        _cabinetDoors[index].selectExited.AddListener(OnCabinetDoorStop);
    }

    private void SetComboLock()
    {
        _comboLockSource = _comboLock.gameObject.AddComponent<AudioSource>();
        _unlockClip = _comboLock.GetUnlockClip();
        _comboButtonClip = _comboLock.GetComboButtonClip();
        _incorrectClip = _comboLock.GetIncorrectClip();
        CheckClip(ref _unlockClip);
        CheckClip(ref _comboButtonClip);
        CheckClip(ref _incorrectClip);

        _comboLock.UnlockAction += OnComboUnlock;
        _comboLock.IncorrectAction += OnIncorrectCombo;
        _comboLock.ComboButtonPressed += OnComboButtonPress;
    }

    private void SetWall()
    {
        _wallSource = _wall.gameObject.AddComponent<AudioSource>();
        _wallExplosionClip = _wall.GetExplodeWallClip();
        CheckClip(ref _wallExplosionClip);
        _wallSource.clip = _wallExplosionClip;
        _wall.OnDestroy.AddListener(OnDestroyWall);

        if (_wallSocket != null)
        {
            _wallSocketSource = _wallSocket.gameObject.AddComponent<AudioSource>();
            _wallSocketClip = _wall.GetSocketedClip();
            CheckClip(ref _wallSocketClip);
            _wallSocketSource.clip = _wallSocketClip;
            _wallSocket.selectEntered.AddListener(OnWallSocketed);
        }
    }
}