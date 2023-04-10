public class Position {
    public int Row { get; }
    public int Column { get; }

    public Position(int row, int column) {
        Row = row;
        Column = column;
    }

    // Checks to see if the object is a Position, and if it is, checks to see if the row and column are the same
    public override bool Equals(object obj) {

        if (obj is Position other) {
            return Row == other.Row && Column == other.Column;
        }

        return false;
    }

    // Returns a hash code based on the row and column
    public override int GetHashCode() {
        return 8 * Row + Column;
    }
}