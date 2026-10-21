using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Global game state that survives level loads. Lives in the Boot scene.
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameEventSet events;
        [SerializeField] private ChapterDefinition currentChapter;
        [SerializeField] private MissionDefinition currentMission;
        [SerializeField] private int xanaLevel;
        [SerializeField] private List<LyokoCharacter> unlockedCharacters = new List<LyokoCharacter>
        {
            LyokoCharacter.Odd,
            LyokoCharacter.Ulrich,
            LyokoCharacter.Yumi,
            LyokoCharacter.Aelita
        };

        public static GameManager Instance { get; private set; }

        public GameEventSet Events => events;
        public ChapterDefinition CurrentChapter => currentChapter;
        public MissionDefinition CurrentMission => currentMission;
        public int XanaLevel => xanaLevel;
        public IReadOnlyList<LyokoCharacter> UnlockedCharacters => unlockedCharacters;

        // Safe to call from anywhere: does nothing when the Boot scene is not loaded (tests, isolated scenes).
        public static void RaiseEvent(GameEventId id)
        {
            if (Instance == null || Instance.events == null)
            {
                return;
            }

            GameEvent gameEvent = Instance.events.Get(id);
            if (gameEvent != null)
            {
                gameEvent.Raise();
            }
        }

        public void SetChapter(ChapterDefinition chapter)
        {
            currentChapter = chapter;
        }

        public void SetMission(MissionDefinition mission)
        {
            currentMission = mission;
        }

        public void RaiseXanaLevel(int amount = 1)
        {
            xanaLevel = Mathf.Max(0, xanaLevel + amount);
        }

        public bool IsUnlocked(LyokoCharacter character)
        {
            return unlockedCharacters.Contains(character);
        }

        public void Unlock(LyokoCharacter character)
        {
            if (!unlockedCharacters.Contains(character))
            {
                unlockedCharacters.Add(character);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
