using System.Collections.Generic;

public class MoveInfo  {
    // The player that made the move
    public int Player { get; set; }

    // The position that was played
    public Position Position { get; set; }

    // The positions that were flipped by this move
    public List<Position> Outflanked { get; set; }

    public MoveInfo(int player, Position position, List<Position> outflanked) {
        Player = player;
        Position = position;
        Outflanked = outflanked;
    }
}