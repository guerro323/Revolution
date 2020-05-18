# Revolution
### A fast and managed DOD entity-component and serializer framework.
    
The Revolution library has multiple goals:  
- An entity serializer which should be used in realtime applications (eg: networked games, interprocess, interthread communication)
- A bridge of safety between multiple ECS framework. Instead of making a converter for converting entities of a 'ECS A' to 'ECS B', you can instead recreate entities of 'ECS A' to a Revolution World and deserialize them in 'ECS B'.
- Fully managed memory, you do not need to manage the memory of a Revolution World, Revolution will does the job for you.

But Revolution itself is not an ECS framework! There is currently no way to make systems and query entities.

## Quick Start (entity-component side):
````c#
struct MyComponent : IRevolutionComponent {}

// Create a Revolution World
using var world = new RevolutionWorld();

// Create an entity and set a component
var entity = world.CreateEntity();
world.SetComponent(entity, new MyComponent());

// Create an identifiable entity
// One of the use would be to link an entity from another ECS framework
var entityFromDots = world.CreateIdentEntity(myEntityFromUnityDots);
// Or just to give it a name.
var entityWithName = world.CreateIdentEntity("my entity :)");

// Get these identified entity back
var ent0 = world.GetEntityFromIdentity(myEntityFromUnityDots);
var ent1 = world.GetEntityFromIdentity("my entity :)");

// Iterate chunks
foreach (var chunk in world.Chunks)
{
    foreach (var entity in chunk.AsArray)
    {
        // ...
    }
    
    // Get all component array of this chunk.
    // !!!  I do not recommend using this property, instead use chunk.GetComponents<T>()
    //      If you wish to get all component types, use chunk.ComponentTypes property.
    foreach (var (type, componentArray) in chunk.Components)
    {
        if (type == typeof(CustomComponent)) 
        {
            foreach (ref var component in componentArray.GetArray<CustomComponent>())
            {
            }
        }
    }
    
    // Directly get the component array.
    var componentArray = chunk.GetComponents<CustomComponent>();
}

// Get the chunk of an entity
var chunk = world.GetChunk(entity);
````

## Clone
````c#
using var clone = world.Clone();
// the clone will have the same entities identifiers and indexes.
````

## Serializing 
W.I.P.