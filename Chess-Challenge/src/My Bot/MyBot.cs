using System.Runtime;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private static readonly int[] pieceValues=[0,1,3,3,5,7,99];
    
    
    public Move Think(Board board, Timer timer)
    {
        int amWhite=board.IsWhiteToMove?1:-1;
        
         Move[] moves = board.GetLegalMoves();
        int bestMove=0;
        int best=-1000000;
        for (int i = 0; i < moves.Length; i++)
        {
            board.MakeMove(moves[i]);
            
            int e =MinMax(board,3,0)*amWhite;
            if(e>best){
                best=e;
                bestMove=i;
            }
            board.UndoMove(moves[i]);

        }

        return moves[bestMove];
    }
    private static int eval(Board board){
        int total=0;
        Piece p;
        for (int i = 0; i < 64; i++)
        {
            p=board.GetPiece(new Square(i));
            total+=pieceValues[(int)p.PieceType]* (p.IsWhite?1:-1);
        }
        return total;

    }
    private static int MinMax(Board board ,int d,int sign){
        if(d==0){
            return eval(board);
        }
        int amWhite=board.IsWhiteToMove?1:-1;
        
        Move[] moves = board.GetLegalMoves();
        int best=-1000000;
        for (int i = 0; i < moves.Length; i++)
        {
            board.MakeMove(moves[i]);
            
            int e =MinMax(board,d-1,-sign)*amWhite;
            if(e>best){
                best=e;
            }
            board.UndoMove(moves[i]);

        }
        return best*amWhite;

    }

}