using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionHolder : SingletonMono<CollectionHolder>
{
    #region ∑π∆€∑±Ω∫
    protected override bool DontDestroy => true;

    [SerializeField] private CollectionSystem _collectionSystem;
    public CollectionSystem CollectionSystem => _collectionSystem;

    #endregion

    #region √ ±‚»≠
    protected override void Awake()
    {
        base.Awake();
        if (_collectionSystem == null) _collectionSystem = new CollectionSystem();
        _collectionSystem.Initialize(); // µÒº≈≥ ∏Æ ∫ÙµÂ
    }

    #endregion
}