using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; 

public class Socket : XRSocketInteractor
{
    public Position GridPosition;
    public Blob Blob;

    public bool Hoovering = false;
    //public bool AcceptCatchFalling {get{return !Hoovering && Blob == null;}}

    public XRBaseInteractable Interactable {
        get { return GetComponent<XRBaseInteractable>(); }
    }

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

    protected override void OnHoverEntered(HoverEnterEventArgs evetArgs){
        var blobEntering = evetArgs.interactable.GetComponent<Blob>();
        if (blobEntering != null && blobEntering.State == BlobState.PICKED_UP) {
            Hoovering = true;
        }

        base.OnHoverEntered(evetArgs);        
    }
     protected override void OnHoverExited(HoverExitEventArgs evetArgs){
         Hoovering = false;
        base.OnHoverExited(evetArgs);        
    }
    protected override void OnSelectEntering(SelectEnterEventArgs evetArgs)
    {
        //Debug.Log("OnSelectEntering " + interactable);
        base.OnSelectEntering(evetArgs);
    }
    protected override void OnSelectEntered(SelectEnterEventArgs evetArgs)
    {
        Hoovering = false;
        //Debug.Log("onSelectEntered " + interactable);
        base.OnSelectEntered(evetArgs);
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