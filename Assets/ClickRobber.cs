using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickRobber : MonoBehaviour
{

    //
    //Nuevo script para gestionar click en la ficha robber en caso de que esta se encuentre en una posicion alcanzable
    //

    public GameObject robber;
    private Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<Controller>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Verificamos si se ha hecho clic en algo
            if (Physics.Raycast(ray, out hit))
            {
                // ¿Clic sobre el robber?
                if (hit.collider.gameObject == robber)
                {
                    int tileIndex = robber.GetComponent<RobberMove>().currentTile;

                    // ¿La casilla actual del ladrón es catchable?
                    if (controller.tiles[tileIndex].catchable)
                    {
                        controller.ClickOnTile(tileIndex);
                    }
                }
            }
        }
    }
}
