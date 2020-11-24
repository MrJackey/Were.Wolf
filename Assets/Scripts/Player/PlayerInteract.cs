using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    //Trying to check if the player now can pick up a box, but it gives me a 
    // NullReferenceException on row 42, saying that reference is not set to
    // an instance of an object..

    [SerializeField] private BoxCollider2D interactionTrigger;
    [SerializeField] private GameObject playerHand;
    private GameObject carryItem;

    private bool isItemInRange = false, isCarryingItem = false;


    private void Update() {
        if (Input.GetButtonDown("Interact")) {
            if (isItemInRange && !isCarryingItem) {
                PickUpItem();
            }
            else if (isCarryingItem) {
                DropItem();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Box")) {
            carryItem = collider.gameObject;
            isItemInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if (collider.CompareTag("Box")) {
            isItemInRange = false;
        }
    }

    private void PickUpItem() {
        isCarryingItem = true;
        carryItem.GetComponent<Box>().boxCollider.enabled = false;
        carryItem.GetComponent<Rigidbody2D>().isKinematic = true;
        carryItem.transform.parent = playerHand.transform;
        carryItem.transform.localPosition = Vector3.zero;
        interactionTrigger.enabled = false;
    }

    private void DropItem() {
        isCarryingItem = false;
        carryItem.GetComponent<Box>().boxCollider.enabled = true;
        carryItem.GetComponent<Rigidbody2D>().isKinematic = false;
        carryItem.transform.parent = null;
        interactionTrigger.enabled = true;
    }
}
