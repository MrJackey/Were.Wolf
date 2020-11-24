using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
   [SerializeField] public BoxCollider2D boxCollider;

   private Rigidbody2D rb2D;

   private Vector2 startPos;

   private void Start() {
       startPos = transform.position;
       rb2D = GetComponent<Rigidbody2D>();
   }

   private void OnCollisionEnter2D(Collision2D collision) {
       if (collision.collider.CompareTag("Spike")) {
           transform.position = startPos;
           rb2D.velocity = Vector2.zero;
       }
   }
}
