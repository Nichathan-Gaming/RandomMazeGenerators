using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class MapLocation
{
    internal int x;
    internal int y;

    internal MapLocation(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

/// <summary>
/// Maze algorithms created with help from : https://en.wikipedia.org/wiki/Maze_generation_algorithm
/// obtained March 9, 2023
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    #region Maze Size
    [Header("Maze Size")]
    public int width = 30;
    public int height = 30;
    public bool hasBorder = true;
    #endregion

    #region mazeValues
    /// <summary>
    /// The byte grid where we generate our maze
    /// 
    /// 1 == wall
    /// 0 == path
    /// </summary>
    byte[,] map;
    /// <summary>
    /// Holds the last Images of the grid
    /// </summary>
    List<Image> images = new List<Image>();
    #endregion

    #region Room Values
    [Header("Room Values")]
    [Range(0, 10)]
    public int numberOfRooms = 0;
    [Range(0, 20)]
    public int minRoomSize = 0;
    [Range(0, 20)]
    public int maxRoomSize = 0;
    [Range(0, 10)]
    public int roomDistanceFromWall = 0;
    #endregion

    #region Generate Values
    [Header("Generate Values")]
    [Header("0-Random, 1-Crawler, 2-Recursive, 3-Wilsons, 4-Prims")]
    [Range(0, 4)]
    public int generationType = 0;
    [SerializeField] GridLayoutGroup gridLayout;
    [SerializeField] GameObject prefab;
    [SerializeField] Transform instantiationLocation;

    [Header("Crawler Values")]
    [Range(0, 20)]
    public int verticalCrawlCount = 5;
    [Range(0, 20)]
    public int horizontalCrawlCount = 5;
    #endregion

    #region Random Seed Area
    [Header("Random Seed Area")]
    public bool useRandomSeed = false;
    public int randomSeed = 30;
    System.Random random;
    #endregion

    public void Regenerate()
    {
        if(images != null && images.Count > 0)
        {
            foreach (Image i in images) if(i!=null)Destroy(i.gameObject);
            images.Clear();
        }

        Generate();
    }

    /// <summary>
    /// Creates a new maze when the 'R' key is pressed
    /// </summary>
    void Generate()
    {
        #region assign default values
        random = useRandomSeed ? new System.Random(randomSeed) : new System.Random();

        map = new byte[width, height];

        for (int i = 0; i < width; i++) for (int j = 0; j < height; j++) map[i, j] = 1;
        #endregion

        //Determine the generation algorithm to use
        switch (generationType)
        {
            case 1:
                CrawlerGenerator();
                break;
            case 2:
                RecursiveGenerator();
                break;
            case 3:
                WilsonsGenerator();
                break;
            case 4:
                PrimsGenerator();
                break;
            default:
                RandomGenerator();
                break;
        }

        AddRoom();

        //Instatioate and display the maze
        if (hasBorder)
        {
            gridLayout.constraintCount = width + 2;

            for (int i = 0; i < width + 2; i++)
            {
                for (int j = 0; j < height + 2; j++)
                {
                    Image image = Instantiate(prefab, instantiationLocation).GetComponent<Image>();

                    images.Add(image);

                    //continue to next int if at border
                    if (i == 0 || j == 0 || i == width + 1 || j == height + 1) continue;

                    image.color = map[i - 1, j - 1] == 1 ? Color.white : Color.black;
                }
            }
        }
        else
        {
            gridLayout.constraintCount = width;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Image image = Instantiate(prefab, instantiationLocation).GetComponent<Image>();

                    image.color = map[i, j] == 1 ? Color.white : Color.black;
                    images.Add(image);
                }
            }
        }

        //Adds rooms to the maze if selected
        void AddRoom()
        {
            for (int i = 0; i < numberOfRooms; i++)
            {
                int startX = random.Next(roomDistanceFromWall, width - roomDistanceFromWall),
                    startY = random.Next(roomDistanceFromWall, height - roomDistanceFromWall),
                    roomWidth = random.Next(minRoomSize, maxRoomSize),
                    roomHeight = random.Next(minRoomSize, maxRoomSize);

                for (int x = startX; x < width - roomDistanceFromWall && x < startX + roomWidth; x++) for (int y = startY; y < height - roomDistanceFromWall && y < startY + roomHeight; y++) map[x, y] = 0;
            }
        }
    }

    int CountSquareNeighbors(byte[,] rMap, int x, int y)
    {
        int count = 0;

        if (x <= 0 || x >= width + 1 || y <= 0 || y >= height + 1) return 5;

        if (rMap[x - 1, y] == 0) count++;
        if (rMap[x + 1, y] == 0) count++;
        if (rMap[x, y - 1] == 0) count++;
        if (rMap[x, y + 1] == 0) count++;

        return count;
    }

    /// <summary>
    /// Generate purely random values for the maze
    /// 
    /// - Assign a seed to have the same random values every time
    /// </summary>
    void RandomGenerator()
    {
        for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) map[x, y] = (byte)random.Next(0, 2);
    }

    /// <summary>
    /// Generates a random maze by crawling through the maze horizontally and/or vertically verticalCrawlCount/horizontalCrawlCount times until a wall is reached.
    /// 
    /// - Assign verticalCrawlCount/horizontalCrawlCount in the inspector
    /// </summary>
    void CrawlerGenerator()
    {
        int currV = verticalCrawlCount,
            currH = horizontalCrawlCount;

        while (currH > 0 || currV > 0)
        {
            if (currV-- > 0) CrawlVertically();
            if (currH-- > 0) CrawlHorizontally();
        }

        void CrawlVertically()
        {
            Crawl(random.Next(1, width), -1, 2, 1, 0, 2);
        }

        void CrawlHorizontally()
        {
            Crawl(1, 0, 2, random.Next(1, height), -1, 2);
        }

        void Crawl(int x, int xMinMove, int xMaxMove, int y, int yMinMove, int yMaxMove)
        {
            while (true)
            {
                map[x, y] = 0;

                if (random.Next(0, 2) == 0)
                {
                    x += random.Next(xMinMove, xMaxMove);
                }
                else
                {
                    y += random.Next(yMinMove, yMaxMove);
                }

                if (x < 0 || x > width - 1 || y < 0 || y > height - 1) break;
            }
        }
    }

    /// <summary>
    /// Recursively run through the maze until almost all parts of the maze have been filled
    /// 
    /// - Only creates a non-connected path
    /// </summary>
    void RecursiveGenerator()
    {
        byte[,] rMap = new byte[width + 2, height + 2];
        for (int i = 0; i < width + 2; i++) for (int j = 0; j < height + 2; j++) rMap[i, j] = 1;

        InnerGenerate(random.Next(1, width + 1), random.Next(1, height + 1));

        //assign rMap to map
        for (int i = 0; i < width; i++) for (int j = 0; j < height; j++) map[i, j] = rMap[i + 1, j + 1];

        void InnerGenerate(int x, int y)
        {
            if (CountSquareNeighbors(rMap, x, y) > 1) return;
            rMap[x, y] = 0;

            List<MapLocation> mapLocations = new List<MapLocation>()
            {
                new MapLocation(1,0),
                new MapLocation(0,1),
                new MapLocation(-1,0),
                new MapLocation(0, -1)
            };

            Shuffle();

            foreach (MapLocation mapLocation in mapLocations) InnerGenerate(x + mapLocation.x, y + mapLocation.y);

            //shuffles the directions to look in
            void Shuffle()
            {
                List<MapLocation> newLocs = new List<MapLocation>();

                while (mapLocations.Count > 0)
                {
                    int rand = random.Next(0, mapLocations.Count);

                    newLocs.Add(mapLocations[rand]);

                    mapLocations.RemoveAt(rand);
                }

                mapLocations.AddRange(newLocs);
            }
        }
    }

    /// <summary>
    /// Generate a random maze following the Wilson algorithm
    /// 
    /// Wikipedia Link : https://en.wikipedia.org/wiki/Loop-erased_random_walk
    /// obtained March 9, 2023
    /// </summary>
    void WilsonsGenerator()
    {
        byte[,] rMap = new byte[width + 2, height + 2];
        for (int i = 0; i < width + 2; i++) for (int j = 0; j < height + 2; j++) rMap[i, j] = 1;

        List<MapLocation> mapLocations = new List<MapLocation>()
        {
            new MapLocation(1,0),
            new MapLocation(0,1),
            new MapLocation(-1,0),
            new MapLocation(0, -1)
        };

        List<MapLocation> unusedMapLocations = new List<MapLocation>();

        int startX = random.Next(1, width + 1),
            startY = random.Next(1, height + 1);

        rMap[startX, startY] = 2;

        //prevent excesive looping
        int limit = 0;

        int lastCount = -1;
        int currentCount = GetAvailableCells();

        while (currentCount > 1 && (limit++ < 5000 || lastCount != currentCount))
        {
            lastCount = currentCount;

            RandomWalk();

            currentCount = GetAvailableCells();
        }

        //assign rMap to map
        for (int i = 0; i < width; i++) for (int j = 0; j < height; j++) map[i, j] = rMap[i + 1, j + 1];

        int GetAvailableCells()
        {
            unusedMapLocations.Clear();

            for (int x = 1; x < width; x++) for (int y = 1; y < height; y++) if (CountSquareMazeNeighbors(x, y) == 0) unusedMapLocations.Add(new MapLocation(x, y));

            return unusedMapLocations.Count;
        }

        int CountSquareMazeNeighbors(int x, int y)
        {
            int count = 0;

            for (int i = 0; i < mapLocations.Count; i++)
            {
                int nextX = x + mapLocations[i].x,
                    nextY = y + mapLocations[i].y;

                if (rMap[nextX, nextY] == 2) count++;
            }

            return count;
        }

        void RandomWalk()
        {
            MapLocation mapLocation = unusedMapLocations[random.Next(0, unusedMapLocations.Count)];

            List<MapLocation> locationsInWalk = new List<MapLocation>()
            {
                mapLocation
            };

            //prevent excesive looping
            int loop = 0;
            bool validPath = false;

            while (mapLocation.x > 0 && mapLocation.x < width + 1 && mapLocation.y < height + 1 && mapLocation.y > 0 && !validPath && loop++ < 5000)
            {
                rMap[mapLocation.x, mapLocation.y] = 0;

                int cSMNCount = CountSquareMazeNeighbors(mapLocation.x, mapLocation.y);

                validPath = cSMNCount == 1;

                if (cSMNCount > 1) break;

                int randLocation = random.Next(0, mapLocations.Count);

                MapLocation newMapLocation = new MapLocation(
                    mapLocation.x + mapLocations[randLocation].x,
                    mapLocation.y + mapLocations[randLocation].y
                );

                if (CountSquareNeighbors(rMap, newMapLocation.x, newMapLocation.y) < 2)
                {
                    locationsInWalk.Add(mapLocation);
                    mapLocation = newMapLocation;
                }
            }

            if (validPath)
            {
                rMap[mapLocation.x, mapLocation.y] = 0;

                foreach (MapLocation m in locationsInWalk) rMap[m.x, m.y] = 2;
            }
            else
            {
                foreach (MapLocation m in locationsInWalk) rMap[m.x, m.y] = 1;
            }

            locationsInWalk.Clear();
        }
    }

    /// <summary>
    /// Generate a random maze following the Prims algorithm
    /// 
    /// Wikipedia Link : https://en.wikipedia.org/wiki/Prim%27s_algorithm
    /// obtained March 9, 2023
    /// </summary>
    void PrimsGenerator()
    {
        byte[,] rMap = new byte[width + 2, height + 2];
        for (int i = 0; i < width + 2; i++) for (int j = 0; j < height + 2; j++) rMap[i, j] = 1;

        int x = 1,
            y = 1,
            countLoops = 0;
        rMap[x, y] = 0;

        List<MapLocation> mapLocations = new List<MapLocation>()
        {
            new MapLocation(x+1,y),
            new MapLocation(x-1,y),
            new MapLocation(x,y-1),
            new MapLocation(x, y+1)
        };

        while (mapLocations.Count > 0 && countLoops++ < 5000)
        {
            int rLoc = random.Next(0, mapLocations.Count);

            x = mapLocations[rLoc].x;
            y = mapLocations[rLoc].y;

            mapLocations.RemoveAt(rLoc);

            if (CountSquareNeighbors(rMap, x, y) == 1)
            {
                rMap[x, y] = 0;

                mapLocations.Add(new MapLocation(x + 1, y));
                mapLocations.Add(new MapLocation(x - 1, y));
                mapLocations.Add(new MapLocation(x, y - 1));
                mapLocations.Add(new MapLocation(x, y + 1));
            }
        }

        //assign rMap to map
        for (int i = 0; i < width; i++) for (int j = 0; j < height; j++) map[i, j] = rMap[i + 1, j + 1];
    }
}
