namespace Core.Cards {
    public class CardJson
    {
        public string Id;
        public int Count;
        public bool InDeck;

        public CardJson(string id, int count, bool inDeck) 
        {
            Id = id;
            Count = count;
            InDeck = inDeck;
        }
    }
}
