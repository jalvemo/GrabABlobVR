using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; 

public class Socket : XRSocketInteractor
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnSelectEntering(XRBaseInteractable interactable)
    {
        //Debug.Log("OnSelectEntering " + interactable);
        base.OnSelectEntering(interactable);
    }
    protected override void OnSelectEntered(XRBaseInteractable interactable)
    {
        //Debug.Log("onSelectEntered " + interactable);
        base.OnSelectEntered(interactable);
    }

    private void StoreArrow(XRBaseInteractable interactable)
    {

    }

    private void TryToReleaseArrow(XRBaseInteractor interactor)
    {

    }

    private void ForceDeselect()
    {

    }

    private void ReleaseArrow()
    {

    }

    public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride
    {
        get { return XRBaseInteractable.MovementType.Instantaneous; }
    }
}