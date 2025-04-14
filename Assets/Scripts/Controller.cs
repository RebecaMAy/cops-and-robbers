using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    public Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    public int state; // cambio a clase publica para ser accesible desde RobberMove
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {       
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    /*public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's

        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)

        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes

    }*/

    public void InitAdjacencyLists()
    {
        int[,] matriz = new int[Constants.NumTiles, Constants.NumTiles]; //Esta matriz sirve para guardar relaciones entre casillas, no posiciones físicas.

        // Inicializamos la matriz a ceros
        for (int i = 0; i < Constants.NumTiles; i++)
            for (int j = 0; j < Constants.NumTiles; j++)
                matriz[i, j] = 0;

        // Para cada casilla, conectamos con adyacentes válidas
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            int fila = i / Constants.TilesPerRow;
            int col = i % Constants.TilesPerRow;

            // ARRIBA
            if (fila > 0)
            {
                int up = i - Constants.TilesPerRow;
                matriz[i, up] = 1;
            }
            // ABAJO
            if (fila < Constants.TilesPerRow - 1)
            {
                int down = i + Constants.TilesPerRow;
                matriz[i, down] = 1;
            }
            // IZQUIERDA
            if (col > 0)
            {
                int left = i - 1;
                matriz[i, left] = 1;
            }
            // DERECHA
            if (col < Constants.TilesPerRow - 1)
            {
                int right = i + 1;
                matriz[i, right] = 1;
            }
        }

        // Rellenamos la lista de adyacencia de cada casilla
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                if (matriz[i, j] == 1)
                {
                    tiles[i].adjacency.Add(j);
                }
            }
        }
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    // ORIGINAL
    /*public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        //TODO: Cambia el código de abajo para hacer lo siguiente
        //- Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        //- Movemos al caco a esa casilla
        //- Actualizamos la variable currentTile del caco a la nueva casilla
        
        robber.GetComponent<RobberMove>().MoveToTile(tiles[robber.GetComponent<RobberMove>().currentTile]);
    }*/

    //V1.0
    /*public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        Tile startTile = tiles[clickedTile];
        ResetTiles();

        // BFS para calcular casillas alcanzables (como en FindSelectableTiles)
        startTile.visited = true;
        startTile.distance = 0;
        startTile.parent = null;

        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(startTile);

        List<Tile> reachable = new List<Tile>();

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            foreach (int adj in current.adjacency)
            {
                Tile neighbor = tiles[adj];

                if (!neighbor.visited)
                {
                    neighbor.visited = true;
                    neighbor.parent = current;
                    neighbor.distance = current.distance + 1;

                    if (neighbor.distance <= Constants.Distance)
                    {
                        queue.Enqueue(neighbor);

                        if (neighbor.numTile != clickedTile) // Excluimos la posición actual
                        {
                            reachable.Add(neighbor);
                        }
                    }
                }
            }
        }

        // Elegimos una casilla aleatoria entre las alcanzables
        if (reachable.Count > 0)
        {
            Tile destino = reachable[Random.Range(0, reachable.Count)];

            robber.GetComponent<RobberMove>().MoveToTile(destino);
            robber.GetComponent<RobberMove>().currentTile = destino.numTile;
        }
        else
        {
            // Si no hay ninguna alcanzable distinta, se queda en su sitio
            robber.GetComponent<RobberMove>().MoveToTile(startTile);
        }
    }*/

    //V1.1
    /*public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        Tile startTile = tiles[clickedTile];
        ResetTiles();

        // BFS para encontrar casillas alcanzables
        startTile.visited = true;
        startTile.distance = 0;
        startTile.parent = null;

        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(startTile);

        List<Tile> reachable = new List<Tile>();

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            foreach (int adj in current.adjacency)
            {
                Tile neighbor = tiles[adj];

                if (!neighbor.visited)
                {
                    neighbor.visited = true;
                    neighbor.parent = current;
                    neighbor.distance = current.distance + 1;

                    if (neighbor.distance <= Constants.Distance)
                    {
                        queue.Enqueue(neighbor);

                        if (neighbor.numTile != clickedTile)
                            reachable.Add(neighbor);
                    }
                }
            }
        }

        // Obtener casillas donde están los policías y sus adyacentes
        HashSet<int> dangerZones = new HashSet<int>();
        foreach (GameObject cop in cops)
        {
            int copTile = cop.GetComponent<CopMove>().currentTile;
            dangerZones.Add(copTile);
            foreach (int adj in tiles[copTile].adjacency)
                dangerZones.Add(adj);
        }

        // Filtrar casillas seguras
        List<Tile> safeTiles = new List<Tile>();
        foreach (Tile tile in reachable)
        {
            if (!dangerZones.Contains(tile.numTile))
                safeTiles.Add(tile);
        }

        Tile destino = null;

        if (safeTiles.Count > 0)
        {
            destino = safeTiles[Random.Range(0, safeTiles.Count)];
        }
        else if (reachable.Count > 0)
        {
            destino = reachable[Random.Range(0, reachable.Count)];
        }

        if (destino != null)
        {
            robber.GetComponent<RobberMove>().MoveToTile(destino);
            robber.GetComponent<RobberMove>().currentTile = destino.numTile;
        }
        else
        {
            // No hay lugar a donde moverse
            robber.GetComponent<RobberMove>().MoveToTile(startTile);
        }
    }*/

    //VERSION FINAL
    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        Tile startTile = tiles[clickedTile];
        ResetTiles();

        // BFS desde el ladrón para obtener casillas alcanzables
        startTile.visited = true;
        startTile.distance = 0;
        startTile.parent = null;

        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(startTile);

        List<Tile> reachable = new List<Tile>();

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            foreach (int adj in current.adjacency)
            {
                Tile neighbor = tiles[adj];

                if (!neighbor.visited)
                {
                    neighbor.visited = true;
                    neighbor.parent = current;
                    neighbor.distance = current.distance + 1;

                    if (neighbor.distance <= Constants.Distance)
                    {
                        queue.Enqueue(neighbor);
                        if (neighbor.numTile != clickedTile)
                            reachable.Add(neighbor);
                    }
                }
            }
        }

        // Calculamos la distancia mínima a los policías para cada casilla alcanzable
        Tile bestTile = null;
        int maxMinDistance = -1;

        foreach (Tile tile in reachable)
        {
            int minDistToCop = int.MaxValue;

            foreach (GameObject cop in cops)
            {
                int copTileIndex = cop.GetComponent<CopMove>().currentTile;

                // BFS desde la casilla actual hacia cada policía
                Queue<Tile> bfsQueue = new Queue<Tile>();
                bool[] visited = new bool[Constants.NumTiles];
                int[] distance = new int[Constants.NumTiles];

                bfsQueue.Enqueue(tile);
                visited[tile.numTile] = true;
                distance[tile.numTile] = 0;

                while (bfsQueue.Count > 0)
                {
                    Tile current = bfsQueue.Dequeue();

                    if (current.numTile == copTileIndex)
                    {
                        minDistToCop = Mathf.Min(minDistToCop, distance[current.numTile]);
                        break;
                    }

                    foreach (int adj in current.adjacency)
                    {
                        if (!visited[adj])
                        {
                            visited[adj] = true;
                            distance[adj] = distance[current.numTile] + 1;
                            bfsQueue.Enqueue(tiles[adj]);
                        }
                    }
                }
            }

            // Elegimos la casilla más alejada del policía más cercano
            if (minDistToCop > maxMinDistance)
            {
                maxMinDistance = minDistToCop;
                bestTile = tile;
            }
        }

        if (bestTile != null)
        {
            robber.GetComponent<RobberMove>().MoveToTile(bestTile);
            robber.GetComponent<RobberMove>().currentTile = bestTile.numTile;
        }
        else
        {
            // No hay lugar mejor, se queda donde está
            robber.GetComponent<RobberMove>().MoveToTile(startTile);
        }
    }

    public void EndGame(bool end)
    {
        if(end && state!=4)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    // ORIGINAL
    /*public void FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;        

        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        for(int i = 0; i < Constants.NumTiles; i++)
        {
            tiles[i].selectable = true;
        }


    }*/

    public void FindSelectableTiles(bool cop)
    {
        ResetTiles();

        int indexcurrentTile;
        int otherCopTile = -1;

        //Guarda la posición del otro policía (el que no estás moviendo).

       //Esto se usa para que durante el BFS, el policía actual no pueda atravesar ni terminar su movimiento en la casilla ocupada por el otro policía.

        if (cop)
        {
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
            otherCopTile = cops[1 - clickedCop].GetComponent<CopMove>().currentTile;
        }
        else
        {
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;
        }

        Tile startTile = tiles[indexcurrentTile];
        startTile.visited = true;
        startTile.distance = 0;
        startTile.parent = null;

        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(startTile);

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            foreach (int adj in current.adjacency)
            {
                Tile neighbor = tiles[adj];

                // Si ya fue visitado, ignoramos
                if (neighbor.visited)
                    continue;

                // Si es policía, comprobamos que no pasamos por la casilla del otro
                if (cop && current.numTile == otherCopTile)
                    continue;

                // Nuevo: Evitar que un policía se mueva a la casilla del otro
                if (cop && neighbor.numTile == otherCopTile)
                    continue;

                neighbor.visited = true;
                neighbor.parent = current;
                neighbor.distance = current.distance + 1;

                if (neighbor.distance <= Constants.Distance)
                {
                    queue.Enqueue(neighbor);

                    // Evitamos marcar como selectable su propia casilla
                    if (neighbor.numTile != indexcurrentTile)
                    {
                        neighbor.selectable = true;

                        if (cop && neighbor.numTile == robber.GetComponent<RobberMove>().currentTile)
                        {
                            neighbor.catchable = true;
                        }
                    }
                }
            }

        }
    }










}
