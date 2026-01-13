# 🛠 Collection System

## 🎯 Goals
> **Players**
> 
> Allow the players to collect any kind of objects.

> **Developers**
> 
> Allow developers to create objects to collect while being able to add and override any kind of behaviours.

## 🧩 Summary
- **Type**
  - Gameplay
- **Dependencies**
  - VInspector *Remove this dependency* *(only used in 'Collectable.cs' script)*
  - VV.Utility

## 🧱 Architecture
### [Diagram](https://www.figma.com/board/Dyb5c3L3XTcxIcthmgLzRg/Forindustrie-UML?node-id=283-691&t=d8oZ2a3GjXh3yJPl-0)

#### [VV Diagram](https://www.figma.com/board/Dyb5c3L3XTcxIcthmgLzRg/Forindustrie-UML?node-id=332-855&t=d8oZ2a3GjXh3yJPl-0)

![VV Collectable UML.jpg](Editor/Documentation/VV%20Collectable%20UML.jpg)

#### [Forindustrie Diagram](https://www.figma.com/board/Dyb5c3L3XTcxIcthmgLzRg/Forindustrie-UML?node-id=283-691&t=d8oZ2a3GjXh3yJPl-0)

![Forindustrie Collectable UML](Editor/Documentation/Forindustrie-Collectable-UML.jpg)

### Script Structure

    Scripts
    ├── Collectables
        ├── Editor
            ├── CollectionBuilder.cs
        ├── Data
            ├── CollectableSOBase.cs
            ├── CollectableStoredData.cs
            ├── CollectionSO.cs
        ├── Settings
            ├── CollectionsSettings.cs
            ├── CollectionsSettingsProvider.cs
        ├── Network
            ├── CollectablePayloadHandler.cs
            ├── CollectableSocketController.cs
            ├── CollectableSocketHandler.cs
        ├── Handler
            ├── CollectionEventHandler.cs
            ├── CollectionStatsHandler.cs
        ├── Collectable.cs
        ├── CollectableBaseBehaviour.cs
        ├── CollectionManager.cs
        ├── FICollectableBehaviour.cs
        ├── ForindustrieCollectable.cs
        ├── LeafCollectableBehaviour.cs
        └── RuntimeCollection.cs
</span>

## ⚙️ Internal 
### Internal Workflow

#### Collect Workflow

![Collect Sequence Diagram.svg](Editor/Documentation/Collect%20Sequence%20Diagram.svg)

## 🔧 Unity Configuration
1. Create a Collection SO
![Workflow-step-1.png](Editor/Documentation/Workflow-step-1.png)

2. Configure the name and score, you can add a template if needed
![Workflow-step-2.png](Editor/Documentation/Workflow-step-2.png)

3. Configure the prefab\
You need at least to add the Collectable and Collectable behaviour components
![Workflow-step-3.png](Editor/Documentation/Workflow-step-3.png)

4. Place the collectables in your game

5. Generate the collectables
![Workflow-step-5.png](Editor/Documentation/Workflow-step-5.png)

6. In the Project Settings > VV > Collecting
Add the collection SO to the list and generate the enum (if the collection is new)
![Workflow-step-6.png](Editor/Documentation/Workflow-step-6.png)

## 🔗 Dependancies
- VInspector *Remove this dependency* *(only used in 'Collectable.cs' script)*
- VV.Utility

## 🧪 Tests
> - [ ] Implement tests
> - **Unitaire**
> - **Integration**
>   - [ ] Test the collectable SO generation
> - **Gameplay**

## 🚀 Limits & Optimisations

No limits reached yet.

## 📌 Notes
Events are messy

