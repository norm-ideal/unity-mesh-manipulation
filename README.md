# Unity Mesh Manipulation Test

This is a repository for testing unity Mesh features. It includes the direct access to vertices, normals, uvs and triangles.

# Current Update

* Keep the triangle where all the vertices are below the plane
* If only one vertices are below the plane, move other two vertices onto the plane
* If either or both of two vertices has already been moved toward another point, create new point(s) and a new triangle

Black triangles are newly created ones.

![adding triangles](documents/addition_b1.png)
