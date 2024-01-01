using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using VRC.SDKBase;
using VRC.Udon;

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class IntKeyBoolDictionary : UdonSharpBehaviour
    {
        int[][] _keys;
        bool[][] _values;
        int _size;
        public int Count => _size;

        public int[] GenerateKeysArray(){
            int[] keys=new int[_size];
            int index=0;
            for(int i=0;i<_keys.Length;i++){
                var hashKeys=_keys[i];
                if(i==0){
                    for(int j=0;j<hashKeys.Length;j++){
                        if(hashKeys[j]!=int.MaxValue){
                            keys[index]=hashKeys[j];
                            index++;
                        }
                    }
                } else {
                    for(int j=0;j<hashKeys.Length;j++){
                        if(hashKeys[j]!=default){
                            keys[index]=hashKeys[j];
                            index++;
                        }
                    }
                }
            }
            return keys;
        }

        const int bucketCount=1;
        public void SetCapacity(int capacity)
        {
            int len=capacity/bucketCount;
            _keys=new int[len][];
            for(int i=0;i<len;i++){
                _keys[i]=new int[bucketCount];
            }

            // 0-value-key 対策
            for(int i=0;i<bucketCount;i++){
                _keys[0][i]=int.MaxValue;
            }

            _values=new bool[len][];
            for(int i=0;i<len;i++){
                _values[i]=new bool[bucketCount];
            }

        }

        public bool HasItem(int key){
            var index=Hash(key);
            var hashKeys=_keys[index];
            if(index==0){
                if(key==int.MaxValue){
                    Debug.LogError("not allowed key value");
                }
            }
            for(int i=0;i<hashKeys.Length;i++){
                if(hashKeys[i]==key){
                    return true;
                }
            }
            return false;
        }

        private int Hash(int key)
        {
            var len= _keys.Length;
            var mod =key % len;
            if(mod>=0){
                return mod; 
            }else {
                return mod+len;
            }
            // return 0 to len-1;
        }

        public bool Add(int key, bool value)
        {
            int index = Hash(key);
            var hashKeys=_keys[index];
            if(key==int.MaxValue){
                Debug.LogError("not allowed key value");
            }
            if(index==0){

                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        Debug.LogError("InkEyBoolDictionary: Key already exists "+ key.ToString());
                    }
                    if(hashKeys[i]==int.MaxValue){
                        _keys[index][i]=key;
                        _values[index][i]=value;
                        _size++;
                        return true;
                    }
                }
            } else {
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        Debug.LogError("InkEyBoolDictionary: Key already exists "+ key.ToString());
                    }
                    if(hashKeys[i]==default){
                        _keys[index][i]=key;
                        _values[index][i]=value;
                        _size++;
                        return true;
                    }
                }
            }
            //Debug.LogWarning("InkEyBoolDictionary Add: Dictionary is full");
            var preLen=hashKeys.Length;
            ExpandList(index);
            _keys[index][preLen]=key;
            _values[index][preLen]=value;
            _size++;
            return true;
        }
        const int expand=5;
        void ExpandList(int index){
            var hashKeys=_keys[index];
            var hashValues=_values[index];
            var len =_keys[index].Length+expand;
            var newKeys=new int[len];
            var newValues=new bool[len];
            for(int i=0;i<_keys[index].Length;i++){
                newKeys[i] = hashKeys[i];
                newValues[i] = hashValues[i];
            }
            if(index==0){
                for(int i=_keys[index].Length;i<len;i++){
                    newKeys[i]=int.MaxValue;
                }
            }
            _keys[index]=newKeys;
            _values[index]=newValues;
        }

        public bool AddOrSetValue(int key,bool value){
            int index = Hash(key);
            var hashKeys=_keys[index];
            if(index==0){
                if(key==int.MaxValue){
                    Debug.LogError("not allowed key value");
                }
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        _keys[index][i]=key;
                        _values[index][i]=value;
                        return true;
                    }
                    if(hashKeys[i]==int.MaxValue){
                        _keys[index][i]=key;
                        _values[index][i]=value;
                        _size++;
                        return true;
                    }
                }
            } else {
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        _keys[index][i]=key;
                        _values[index][i]=value;
                        return true;
                    }
                    if(hashKeys[i]==default){
                        _keys[index][i]=key;
                        _values[index][i]=value;
                        _size++;
                        return true;
                    }
                }
            }
            //Debug.LogError("InkEyBoolDictionary AddOrSetValue: Dictionary is full");
            //return false;
            var preLen=hashKeys.Length;
            ExpandList(index);
           _keys[index][preLen]=key;
            _values[index][preLen]=value;
            _size++;
            return true;
        }

        public void SetValue(int key, bool value)
        {
            int index = Hash(key);
            var hashKeys=_keys[index];
            if(index==0){
                if(key==int.MaxValue){
                    Debug.LogError("not allowed key value");
                }
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        _values[index][i]=value;
                        return;
                    }
                }
            } else {
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        _values[index][i]=value;
                        return;
                    }
                }
            }
            Debug.LogError("Key not found");
        }

        public bool GetValueOrDefault(int key,bool defaultValue){
            int index = Hash(key);
            var hashKeys=_keys[index];
            var hashValues=_values[index];
            if(index==0){
                if(key==int.MaxValue){
                    Debug.LogError("not allowed key value");
                }
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        return hashValues[i];
                    }
                }
            } else {
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        return hashValues[i];
                    }
                }
            }
            return defaultValue;
        }
        public bool GetValue(int key)
        {
            int index = Hash(key);
            var hashKeys=_keys[index];
            var hashValues=_values[index];
            if(index==0){
                if(key==int.MaxValue){
                    Debug.LogError("not allowed key value");
                }
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        return hashValues[i];
                    }
                }
            } else {
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        return hashValues[i];
                    }
                }
            }
            Debug.LogError("Key not found");
            return default;
        }

        public void TryRemoveBatch(int[] keys,bool[] result){
            for(int i=0;i<keys.Length;i++){
                result[i]=TryRemove(keys[i]);
            }
        }

        public bool TryRemove(int key){
            int index = Hash(key);
            var hashKeys=_keys[index];
            if(index==0){
                if(key==int.MaxValue){
                    Debug.LogError("not allowed key value");
                }
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        hashKeys[i]=int.MaxValue;
                        _size--;
                        return true;
                    }
                }
            } else {
                for(int i=0;i<hashKeys.Length;i++){
                    if(hashKeys[i]==key){
                        hashKeys[i]=default;
                        _size--;
                        return true;
                    }
                }
            }
            return false;
        }
        public void Remove(int key)
        {
            if(!TryRemove(key))Debug.LogError("Key not found");
        }
    }