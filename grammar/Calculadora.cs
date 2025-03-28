using System.Runtime.Serialization;
using com.calitha.commons;

namespace com.calitha.goldparser
{

    [Serializable()]
    public class SymbolException : System.Exception
    {
        public SymbolException(string message) : base(message)
        {
        }

        public SymbolException(string message,
            Exception inner) : base(message, inner)
        {
        }

        protected SymbolException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

    }

    [Serializable()]
    public class RuleException : System.Exception
    {

        public RuleException(string message) : base(message)
        {
        }

        public RuleException(string message,
                             Exception inner) : base(message, inner)
        {
        }

        protected RuleException(SerializationInfo info,
                                StreamingContext context) : base(info, context)
        {
        }

    }

    enum SymbolConstants : int
    {
        SYMBOL_EOF = 0, // (EOF)
        SYMBOL_ERROR = 1, // (Error)
        SYMBOL_WHITESPACE = 2, // Whitespace
        SYMBOL_DIVIDIR = 3, // DIVIDIR
        SYMBOL_LPAREN = 4, // LPAREN
        SYMBOL_MAS = 5, // MAS
        SYMBOL_NUMBER = 6, // NUMBER
        SYMBOL_PI = 7, // PI
        SYMBOL_POR = 8, // POR
        SYMBOL_RAIZ = 9, // RAIZ
        SYMBOL_RESTA = 10, // RESTA
        SYMBOL_RPAREN = 11, // RPAREN
        SYMBOL_SIN = 12, // SIN
        SYMBOL_E = 13, // <E>
        SYMBOL_F = 14, // <F>
        SYMBOL_T = 15  // <T>
    };

    enum RuleConstants : int
    {
        RULE_E_MAS = 0, // <E> ::= <E> MAS <T>
        RULE_E_RESTA = 1, // <E> ::= <E> RESTA <T>
        RULE_E = 2, // <E> ::= <T>
        RULE_T_POR = 3, // <T> ::= <T> POR <F>
        RULE_T_DIVIDIR = 4, // <T> ::= <T> DIVIDIR <F>
        RULE_T = 5, // <T> ::= <F>
        RULE_F_LPAREN_RPAREN = 6, // <F> ::= LPAREN <E> RPAREN
        RULE_F_SIN_LPAREN_RPAREN = 7, // <F> ::= SIN LPAREN <E> RPAREN
        RULE_F_RAIZ_LPAREN_RPAREN = 8, // <F> ::= RAIZ LPAREN <E> RPAREN
        RULE_F_PI = 9, // <F> ::= PI
        RULE_F_NUMBER = 10  // <F> ::= NUMBER
    };

    public class MyParser
    {
        private LALRParser parser;

        public MyParser(string filename)
        {
            FileStream stream = new FileStream(filename,
                                               FileMode.Open,
                                               FileAccess.Read,
                                               FileShare.Read);
            Init(stream);
            stream.Close();
        }

        public MyParser(string baseName, string resourceName)
        {
            byte[] buffer = ResourceUtil.GetByteArrayResource(
                System.Reflection.Assembly.GetExecutingAssembly(),
                baseName,
                resourceName);
            MemoryStream stream = new MemoryStream(buffer);
            Init(stream);
            stream.Close();
        }

        public MyParser(Stream stream)
        {
            Init(stream);
        }

        private void Init(Stream stream)
        {
            CGTReader reader = new CGTReader(stream);
            parser = reader.CreateNewParser();
            parser.TrimReductions = false;
            parser.StoreTokens = LALRParser.StoreTokensMode.NoUserObject;

            parser.OnReduce += new LALRParser.ReduceHandler(ReduceEvent);
            parser.OnTokenRead += new LALRParser.TokenReadHandler(TokenReadEvent);
            parser.OnAccept += new LALRParser.AcceptHandler(AcceptEvent);
            parser.OnTokenError += new LALRParser.TokenErrorHandler(TokenErrorEvent);
            parser.OnParseError += new LALRParser.ParseErrorHandler(ParseErrorEvent);
        }

        public void Parse(string source)
        {
            parser.Parse(source);

        }

        private void TokenReadEvent(LALRParser parser, TokenReadEventArgs args)
        {
            try
            {
                args.Token.UserObject = CreateObject(args.Token);
            }
            catch (Exception e)
            {
                args.Continue = false;
                //todo: Report message to UI?
            }
        }

        private Object CreateObject(TerminalToken token)
        {
            switch (token.Symbol.Id)
            {
                case (int)SymbolConstants.SYMBOL_EOF:
                    //(EOF)
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_ERROR:
                    //(Error)
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_WHITESPACE:
                    //Whitespace
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_DIVIDIR:
                    //DIVIDIR
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_LPAREN:
                    //LPAREN
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_MAS:
                    //MAS
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_NUMBER:
                    //NUMBER
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_PI:
                    //PI
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_POR:
                    //POR
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_RAIZ:
                    //RAIZ
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_RESTA:
                    //RESTA
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_RPAREN:
                    //RPAREN
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_SIN:
                    //SIN
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_E:
                    //<E>
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_F:
                    //<F>
                    //todo: Create a new object that corresponds to the symbol
                    return null;

                case (int)SymbolConstants.SYMBOL_T:
                    //<T>
                    //todo: Create a new object that corresponds to the symbol
                    return null;

            }
            throw new SymbolException("Unknown symbol");
        }

        private void ReduceEvent(LALRParser parser, ReduceEventArgs args)
        {
            try
            {
                args.Token.UserObject = CreateObject(args.Token);
            }
            catch (Exception e)
            {
                args.Continue = false;
                //todo: Report message to UI?
            }
        }

        public static Object CreateObject(NonterminalToken token)
        {
            switch (token.Rule.Id)
            {
                case (int)RuleConstants.RULE_E_MAS:
                    //<E> ::= <E> MAS <T>
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_E_RESTA:
                    //<E> ::= <E> RESTA <T>
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_E:
                    //<E> ::= <T>
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_T_POR:
                    //<T> ::= <T> POR <F>
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_T_DIVIDIR:
                    //<T> ::= <T> DIVIDIR <F>
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_T:
                    //<T> ::= <F>
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_F_LPAREN_RPAREN:
                    //<F> ::= LPAREN <E> RPAREN
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_F_SIN_LPAREN_RPAREN:
                    //<F> ::= SIN LPAREN <E> RPAREN
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_F_RAIZ_LPAREN_RPAREN:
                    //<F> ::= RAIZ LPAREN <E> RPAREN
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_F_PI:
                    //<F> ::= PI
                    //todo: Create a new object using the stored user objects.
                    return null;

                case (int)RuleConstants.RULE_F_NUMBER:
                    //<F> ::= NUMBER
                    //todo: Create a new object using the stored user objects.
                    return null;

            }
            throw new RuleException("Unknown rule");
        }

        private void AcceptEvent(LALRParser parser, AcceptEventArgs args)
        {
            //todo: Use your fully reduced args.Token.UserObject
        }

        private void TokenErrorEvent(LALRParser parser, TokenErrorEventArgs args)
        {
            string message = "Token error with input: '" + args.Token.ToString() + "'";
            //todo: Report message to UI?
        }

        private void ParseErrorEvent(LALRParser parser, ParseErrorEventArgs args)
        {
            string message = "Parse error caused by token: '" + args.UnexpectedToken.ToString() + "'";
            //todo: Report message to UI?
        }


    }
}
