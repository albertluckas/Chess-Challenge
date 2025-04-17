using ChessChallenge.API;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class albot : IChessBot{
    Move bestMove;
    public Move Think(Board board, Timer timer) //Returns the move to play
    {
        
        if(timer.MillisecondsRemaining<10){//I will not lose by time!
            return board.GetLegalMoves()[0];
        }
        
        int csign=board.IsWhiteToMove ? 1:-1;//Figure out what color pieces you are playing with
        if(timer.MillisecondsRemaining<100){ 
            checkTree(board, 2,true,-99999*csign,99999*csign,timer);
            return bestMove;
        }
        if(timer.MillisecondsRemaining<1000){
            checkTree(board, 3,true,-99999*csign,99999*csign,timer);
            return bestMove;
        }
        if(timer.MillisecondsRemaining<30000){
            checkTree(board, 4,true,-99999*csign,99999*csign,timer);
            return bestMove;
        }
        checkTree(board, 5,true,-99999*csign,99999*csign,timer);
        return bestMove;
    }

    public int EvalBoard(Board board){ //Evaluates the current board state
        int activesign=board.IsWhiteToMove ? 1 : -1;
    
        if(board.IsInCheckmate()){ //If checkmate, return "infinite" value
            return activesign*-999999999;
        }
        if(board.IsDraw()){ //*mr incredible* draw is draw
            return 0;
        }
        int evalScore = 0;
        int openingbonus=0;//instead of checking where we are now, keep track of bonuses to add once we know, 
        int endgamebonus=0;//so we only iterate over squares once
        int [] powers=[0,0];
        if(board.IsInCheck()){ //Favors giving checks
            evalScore += activesign*-50;

        }
        for(int i = 0; i < 64; i++){
            Piece piece = board.GetPiece(new Square(i));
            if(!piece.IsNull){
                int pieceColorModifier = piece.IsWhite ? 1 : -1;
                int pieceColorIndicie=piece.IsWhite ? 1 : 0;
                int piecescore=0;
                int hcenterness =(int)(3.5-Math.Abs(3.5-(i%8)));
                int vdevelop=(int)-Math.Abs(3.5-i/8-pieceColorModifier)*2;
                int progress= 4-(4-i/8)*pieceColorModifier;
                if(piece.IsPawn){ //Favors positions where pawns are closer to the other side of the board (and closer to promotion)
                    
                    piecescore = 100+progress*2;
                    openingbonus+=pieceColorModifier*progress*hcenterness;
                    endgamebonus+=pieceColorModifier*progress*progress;
                }
                if(piece.IsBishop){
                piecescore+=333;//idk wikepidia says 3.33
                openingbonus+=pieceColorModifier*vdevelop;//push bishops, but not too far
                }
                if(piece.IsKnight){
                    piecescore+=305;
                    openingbonus+=pieceColorModifier*(hcenterness+vdevelop);//push knights to *gestures vaugly*
                }
                if(piece.IsRook){
                    piecescore+=563;
                    if((i==3&&board.GetKingSquare(piece.IsWhite).Index==2)||(i==5&&board.GetKingSquare(piece.IsWhite).Index==6)
                        ||(i==59&&board.GetKingSquare(piece.IsWhite).Index==58)||(i==61&&board.GetKingSquare(piece.IsWhite).Index==62)){
                        openingbonus+=pieceColorModifier*100;//horrible no good castling bonus
                    }
                }
                if(piece.IsQueen){
                    piecescore+=950;
                    if(progress>0){
                        openingbonus-=pieceColorModifier*200;//no queen rush
                    }
                }
                evalScore+=piecescore*pieceColorModifier;
                powers[pieceColorIndicie]+=piecescore;

                
            }

            
        }
        
        if(powers[0]+powers[1]>=2800){
            evalScore+=openingbonus;

        }
        if(powers[0]<1300&&powers[1]<1300){
            evalScore+=endgamebonus;
        }
        return evalScore;
    }
    
    public int checkTree(Board board, int depth, Boolean root,int bestforcedactive, int bestforcedinactive,Timer timer){
        // bestforced active and bestforced inactive are alpha beta pruning but i did it wierd
        if(depth == 0){ //Base Case
            return EvalBoard(board);
        }
        Move[] moves = board.GetLegalMoves();
        if(depth>2){
            sortmoves(board, moves);//makes pruning more effective, and somewhat prevents procrastinating solidifing advantage
                                    // by perfering captures, all else being equal 
        }
        int csign=board.IsWhiteToMove ? 1:-1;
        if(root){
            bestMove=moves[0];//make sure we, yknow, return a legal move
        }
        int bestEval =-999999*csign;
        if(board.IsInCheckmate()){
            return bestEval;
        }
        foreach(Move move in moves){

            board.MakeMove(move);
            if(move.IsCapture&&timer.MillisecondsRemaining>40*10000){
               depth++; //shame about this seems like its good, but it aint
            }
            int moveEval = checkTree(board, depth-1,false,bestforcedinactive,bestforcedactive,timer); 
            //Continue checking, decrement depth
            board.UndoMove(move);

            if(csign*moveEval > csign*bestEval){ 
                bestEval = moveEval;
                if(root){bestMove = move;}//cursed return of move, because i dont wanna do structs and c sharp wont let me pass a reference

                if(csign*bestforcedactive > csign*bestforcedinactive){ //get pruned idiot
                    return 99999*csign;
                }
                if(csign*bestEval > csign*bestforcedactive){ 
                    bestforcedactive=bestEval;
                }  
            }
                      
        }
        
        return bestEval;
    }   
    
    public void sortmoves(Board board, Move[] moves){//I think you can figure this one out
        int next=0;
        Move t;
        for (int i = 0; i < moves.Length; i++)
        {
            if(moves[i].IsCapture||moves[i].IsCastles){
                t=moves[i];
                moves[i]=moves[next];
                moves[next]=t;
                next++;
            }
        }
    }
}