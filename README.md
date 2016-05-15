# notIridium

![screenshot] (https://www.cs.helsinki.fi/u/saska/notIridium.png)

Small program to generate paths based on CSV data.

The solution is not super optimal but with the small data set it is easily fast enough.

Generating the graph is O(n^2)

Path generation is Dijkstra so O((|V| + |E|)log|V|)

Supports weightings for minimizing "atmospheric disturbances".
