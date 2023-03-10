# RandomMazeGenerators
This tutorial will detail and demonstrate 5 different random maze generators.

## Table of Contents
1. **[Additional Resources](#Additional-Resources)**
2. **[Random Generator Algorithm](#Random-Generator-Algorithm)**
3. **[Crawler Generator Algorithm](#Crawler-Generator-Algorithm)**
4. **[Helper Class](#Helper-Class)**
5. **[Recursive Generator Algorithm](#Recursive-Generator-Algorithm)**
6. **[Wilsons Generator Algorithm](#Wilsons-Generator-Algorithm)**
7. 
Prims Generator Algorithm](#Prims-Generator-Algorithm)**

## Additional Resources
- See a playable example at : [5 Random Maze Generator Algorithms WebGL Playable Example](https://nichathan-gaming.github.io/RandomMazeGenerators/)
- View the source code at : [GitHub Source Code](https://github.com/Nichathan-Gaming/RandomMazeGenerators/blob/master/MazeGenerator.cs)
- For more info on Maze Generation Algorithms see : [Maze Generation Algorithms on Wikipedia](https://en.wikipedia.org/wiki/Maze_generation_algorithm)
- View the video walkthrough at : </br>[![5 Random Maze Generator Algorithms Tutorial](https://img.youtube.com/vi/5H8DvFP_R7I/0.jpg)](https://youtu.be/5H8DvFP_R7I)
</br>**[Back To Top](#RandomMazeGenerators)**

## Random Generator Algorithm
- For the first algorithm that I will introduce, I want to show a purely random option. I feel like this option isn't terribly maze like but I still feel it is important to include.

![ex0](https://user-images.githubusercontent.com/103794085/224266504-db6d8037-dbc5-4f8a-bafc-c54daf9f0f90.png)

```
    /// <summary>
    /// Generate purely random values for the maze
    /// 
    /// - Assign a seed to have the same random values every time
    /// </summary>
    void RandomGenerator()
    {
        for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) map[x, y] = (byte)random.Next(0, 2);
    }
```
</br>**[Back To Top](#RandomMazeGenerators)**

## Crawler Generator Algorithm

- For the second algorithm, I am providing a crawler. This algorithm steps through a grid picking a random direction after each step only ending after it reaches the edge of the grid or after it times out. It is very important to add a timeout to the while loop here otherwise, while very unlikely, you may receive a stack overflow exception.

![ex1](https://user-images.githubusercontent.com/103794085/224267665-86e93224-e620-4d6f-9298-f4308140e1df.png)

```
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
```
</br>**[Back To Top](#RandomMazeGenerators)**

## Helper Class
- In the following 3 examples, I use a helper class caleed CountSquareNeighbors which counts the number of paths to the top, bottom, left and right of a given cell in a maze.

```
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
```
</br>**[Back To Top](#RandomMazeGenerators)**

## Recursive Generator Algorithm
- For my third example, I provide an example of a recursive algorithm which will crawl in random directions saving the path that it travels. Once a path cannot be followed further, it looks back at the steps taken previously for another path to follow. This runs until all possible steps have been made.

![ex2](https://user-images.githubusercontent.com/103794085/224270489-501dce06-0b75-442b-808e-128e3e487131.png)

```
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
```
</br>**[Back To Top](#RandomMazeGenerators)**

## Wilsons Generator Algorithm
- For my second to last example, I use the Wilsons Algorithm which uses random crawls to generate the tree.

![ex3](https://user-images.githubusercontent.com/103794085/224269765-837873d1-982c-41e3-9661-f2a79ac425ed.png)

```
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
```
</br>**[Back To Top](#RandomMazeGenerators)**

## Prims Generator Algorithm
- For my final example, I present my interpretation of the Prims Algorithm which also provides my favorite results. 

![ex4](https://user-images.githubusercontent.com/103794085/224269337-55854fd6-fece-4838-87cb-b140cf66d531.png)

```
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
```
</br>**[Back To Top](#RandomMazeGenerators)**
