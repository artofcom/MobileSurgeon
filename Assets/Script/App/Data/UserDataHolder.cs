using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;

namespace App.Data
{
    public class NoteData
    {
        // public string SurgName { get; set; }
        public float fTimeRate { get; set; }    // 0 ~ 1.0f
        public string Content { get; set; }
    }

    //public class LearningData
    //{
    //  public string Id;
    //  public int Phase;
    //}

    public class UserDataHolder
    {
        public bool ExpertMode { get; set; }            // false to Beginner Mode.
        public int LastRecentLearningId { get; set; }// 
        //public string CurrentLearningId { get; set; }
        //public int IndexSelectedPhase { get; set; }
        //public int IdCurrentPhase { get; set; }         // 1 ~ 
        //public int IdCurrentCategory { get; set; }


        public Dictionary<string, List<NoteData>> DictNotes = new Dictionary<string, List<NoteData>>();
        //public List<LearningData> LearningData = new List<LearningData>();
        public List<int> BookMarkList { get; private set; } = new List<int>();

        EventsGroup Events = new EventsGroup();

        public void Init()
        {
            Load();

            Events.RegisterEvent("OnSurgItemClicked", OnSurgItemClicked);
            Events.RegisterEvent("OnAddToBookMark", OnAddToBookMark);
            Events.RegisterEvent("OnRemoveFromBookMark", OnRemoveFromBookMark);
            //Events.RegisterEvent("OnExpertModeChanged", OnExpertModeChanged);
            //Events.RegisterEvent("OnSurgSectionItemClicked", OnRecentLearningUpdated);
            //Events.RegisterEvent("OnCategoryButtonClicked", OnCategoryButtonClicked);
        }
        public void Clear()
        {
            DictNotes.Clear();
            BookMarkList.Clear();
            ExpertMode = true;
            LastRecentLearningId = 0;

            Save();
        }

        void OnSurgItemClicked(object data)
        {
            Debug.Log("Current Learning Updated.. " + (int)data);
            LastRecentLearningId = (int)data;
            //CurrentLearningId = (string)data;

            PlayerPrefs.SetInt("LastRecentLearningId", LastRecentLearningId);
        }
        void OnAddToBookMark(object data)
        {
            int CPTCode = (int)data;
            for (int q = 0; q < BookMarkList.Count; ++q)
            {
                if (BookMarkList[q] == CPTCode)
                    return;
            }
            BookMarkList.Add(CPTCode);

            SaveBookmarkData();
        }
        void OnRemoveFromBookMark(object data)
        {
            int CPTCode = (int)data;
            BookMarkList.Remove(CPTCode);

            SaveBookmarkData();
        }


        #region Data Load / Save
        public void Load()
        {
            ExpertMode = true;  // for now its always true ---  PlayerPrefs.GetInt("ExpertMode", 0) == 1 ? true : false;
            LastRecentLearningId = PlayerPrefs.GetInt("LastRecentLearningId", 0);

            LoadNoteData();
            LoadBookmarkData();
        }
        void Save()
        {
            PlayerPrefs.SetInt("LastRecentLearningId", LastRecentLearningId);
            SaveBookmarkData();
            SaveNoteData();
        }
        public void LoadNoteData()
        {
            DictNotes.Clear();

            string keyInfo = PlayerPrefs.GetString("NoteKeyInfo", "");
            if (keyInfo.Length == 0) return;

            string[] singleKeyInfo = keyInfo.Split('/');
            for (int k = 0; k < singleKeyInfo.Length; ++k)
            {
                string[] info = singleKeyInfo[k].Split(':');
                if (info.Length != 2) continue;

                int size = int.Parse(info[1]);
                for (int q = 0; q < size; ++q)
                {
                    string noteInfo = PlayerPrefs.GetString($"NoteData-{info[0]}-{q}", "");
                    if (noteInfo.Length == 0)
                        continue;
                    string[] noteData = noteInfo.Split(':');
                    if (noteData.Length != 2)
                        continue;

                    NoteData data = new NoteData();
                    data.fTimeRate = float.Parse(noteData[0]);
                    data.Content = noteData[1];
                    AddNote(info[0], data, false);
                }
            }
        }
        public bool IsBookmarked(int CPTCode)
        {
            for (int q = 0; q < BookMarkList.Count; ++q)
            {
                if (BookMarkList[q] == CPTCode)
                    return true;
            }
            return false;
        }

        public void SaveNoteData()
        {
            return; // for now.

            string keyInfo = "";
            foreach (string key in DictNotes.Keys)
            {
                if (DictNotes[key].Count == 0)
                    continue;

                keyInfo += $"{key}:{DictNotes[key].Count}" + "/";
            }
            keyInfo = keyInfo.Remove(keyInfo.Length - 1);
            PlayerPrefs.SetString("NoteKeyInfo", keyInfo);


            foreach (string key in DictNotes.Keys)
            {
                if (DictNotes[key].Count == 0)
                    continue;

                List<NoteData> listData = DictNotes[key];
                for (int k = 0; k < listData.Count; ++k)
                {
                    PlayerPrefs.SetString($"NoteData-{key}-{k}", $"{listData[k].fTimeRate}:{listData[k].Content}");
                }
            }
        }

        void SaveBookmarkData()
        {
            PlayerPrefs.SetInt("BookmarkCount", BookMarkList.Count);

            for (int q = 0; q < BookMarkList.Count; ++q)
            {
                PlayerPrefs.SetString($"Bookmark-{q}", $"{BookMarkList[q]}");
            }
        }
        void LoadBookmarkData()
        {
            BookMarkList.Clear();

            int count = PlayerPrefs.GetInt("BookmarkCount", 0);
            for (int q = 0; q < count; ++q)
            {
                string data = PlayerPrefs.GetString($"Bookmark-{q}", string.Empty);
                if (string.IsNullOrEmpty(data))
                    continue;

                int CPTCode;
                if (int.TryParse(data, out CPTCode))
                    BookMarkList.Add(CPTCode);
            }
        }
        #endregion


        #region Note Data Handling.
        public void AddNote(string surgName, NoteData data, bool save = true)
        {
            if (DictNotes.ContainsKey(surgName))
                DictNotes[surgName].Add(data);
            else
            {
                List<NoteData> listNotes = new List<NoteData>();
                listNotes.Add(data);
                DictNotes.Add(surgName, listNotes);
            }

            if (save) SaveNoteData();
        }
        public bool RemoveNote(string surgName, float fRate)
        {
            if (!DictNotes.ContainsKey(surgName))
                return false;

            List<NoteData> listNote = DictNotes[surgName];
            for (int k = 0; k < listNote.Count; ++k)
            {
                if (Mathf.Abs(listNote[k].fTimeRate - fRate) <= Mathf.Epsilon)
                {
                    listNote.RemoveAt(k);
                    if (listNote.Count == 0)
                        DictNotes.Remove(surgName);

                    SaveNoteData();
                    return true;
                }
            }
            return false;
        }
        public NoteData GetNote(string surgName, float fRate)
        {
            int idx = GetNoteIndex(surgName, fRate);
            if (idx < 0) return null;

            List<NoteData> listNote = DictNotes[surgName];
            if (idx < listNote.Count)
                return listNote[idx];
            return null;
        }
        public NoteData GetNoteByIndex(string surgName, int idx)
        {
            if (!DictNotes.ContainsKey(surgName))
                return null;

            if (DictNotes[surgName].Count <= idx)
                return null;

            return DictNotes[surgName][idx];
        }
        public int GetNoteIndex(string surgName, float fRate)
        {
            if (!DictNotes.ContainsKey(surgName))
                return -1;

            List<NoteData> listNote = DictNotes[surgName];
            for (int k = 0; k < listNote.Count; ++k)
            {
                if (Mathf.Abs(listNote[k].fTimeRate - fRate) <= Mathf.Epsilon)
                    return k;
            }
            return -1;
        }
        public bool UpdateNote(string surgName, NoteData newData)
        {
            NoteData data = GetNote(surgName, newData.fTimeRate);
            if (data == null)
                return false;

            data.fTimeRate = newData.fTimeRate;
            data.Content = newData.Content;

            SaveNoteData();
            return true;
        }
        #endregion
    }
}