/*
 * PARSER PARA GRAMÁTICAS GOLD - IMPLEMENTACIÓN C#
 * 
 * Este código implementa un parser LALR utilizando el formato Gold Parser
 * para análisis sintáctico de lenguajes y expresiones.
 */

using System;
using System.IO;
using com.calitha.commons;

namespace com.calitha.goldparser
{
    /// <summary>
    /// Excepción para errores de símbolos no reconocidos
    /// </summary>
    [Serializable]
    public class SymbolException : System.Exception
    {
        public SymbolException(string message) : base(message) { }
        public SymbolException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Excepción para errores en reglas gramaticales
    /// </summary>
    [Serializable]
    public class RuleException : System.Exception
    {
        public RuleException(string message) : base(message) { }
        public RuleException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Enumeración de símbolos terminales de la gramática
    /// </summary>
    enum SymbolConstants : int
    {
        SYMBOL_EOF = 0,         // Fin de archivo
        SYMBOL_ERROR = 1,       // Token de error
        SYMBOL_WHITESPACE = 2,  // Espacios en blanco
        SYMBOL_DIVIDIR = 3,     // Operador división (/)
        SYMBOL_LPAREN = 4,      // Paréntesis izquierdo
        SYMBOL_MAS = 5,         // Operador suma (+)
        SYMBOL_NUMBER = 6,      // Números literales
        SYMBOL_PI = 7,          // Constante PI
        SYMBOL_POR = 8,         // Operador multiplicación (*)
        SYMBOL_RAIZ = 9,        // Función raíz cuadrada
        SYMBOL_RESTA = 10,      // Operador resta (-)
        SYMBOL_RPAREN = 11,     // Paréntesis derecho
        SYMBOL_SIN = 12,        // Función seno
        SYMBOL_E = 13,          // No terminal E
        SYMBOL_F = 14,          // No terminal F
        SYMBOL_T = 15           // No terminal T
    }

    /// <summary>
    /// Enumeración de reglas de producción de la gramática
    /// </summary>
    enum RuleConstants : int
    {
        RULE_E_MAS = 0,             // E → E + T
        RULE_E_RESTA = 1,           // E → E - T
        RULE_E = 2,                 // E → T
        RULE_T_POR = 3,             // T → T * F
        RULE_T_DIVIDIR = 4,         // T → T / F
        RULE_T = 5,                 // T → F
        RULE_F_LPAREN_RPAREN = 6,   // F → ( E )
        RULE_F_SIN_LPAREN_RPAREN = 7, // F → sin(E)
        RULE_F_RAIZ_LPAREN_RPAREN = 8, // F → raiz(E)
        RULE_F_PI = 9,              // F → PI
        RULE_F_NUMBER = 10          // F → NUMBER
    }

    /// <summary>
    /// Implementación del parser LALR para gramáticas Gold Parser
    /// </summary>
    public class MyParser
    {
        private readonly LALRParser parser; // Parser LALR (inicializado en constructores)

        // ========== CONSTRUCTORES ========== //

        /// <summary>
        /// Crea un parser desde un archivo .cgt (Compiled Grammar Table)
        /// </summary>
        public MyParser(string filename)
        {
            using FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            this.parser = InitializeParser(stream);
        }

        /// <summary>
        /// Crea un parser desde un recurso embebido
        /// </summary>
        public MyParser(string baseName, string resourceName)
        {
            byte[] buffer = ResourceUtil.GetByteArrayResource(
                System.Reflection.Assembly.GetExecutingAssembly(),
                baseName,
                resourceName);
            using MemoryStream stream = new MemoryStream(buffer);
            this.parser = InitializeParser(stream);
        }

        /// <summary>
        /// Crea un parser desde un stream existente
        /// </summary>
        public MyParser(Stream stream)
        {
            this.parser = InitializeParser(stream);
        }

        // ========== MÉTODOS PRIVADOS ========== //

        /// <summary>
        /// Inicializa y configura el parser desde un stream
        /// </summary>
        private LALRParser InitializeParser(Stream stream)
        {
            var reader = new CGTReader(stream);
            var newParser = reader.CreateNewParser() ?? 
                throw new InvalidOperationException("Error: No se pudo crear el parser");
            
            ConfigureParser(newParser);
            return newParser;
        }

        /// <summary>
        /// Configura las propiedades básicas y eventos del parser
        /// </summary>
        private void ConfigureParser(LALRParser parser)
        {
            // Configuración del comportamiento del parser
            parser.TrimReductions = false;
            parser.StoreTokens = LALRParser.StoreTokensMode.NoUserObject;

            // Asignación de manejadores de eventos
            parser.OnReduce += ReduceEvent;
            parser.OnTokenRead += TokenReadEvent;
            parser.OnAccept += AcceptEvent;
            parser.OnTokenError += TokenErrorEvent;
            parser.OnParseError += ParseErrorEvent;
        }

        // ========== MÉTODOS PÚBLICOS ========== //

        /// <summary>
        /// Ejecuta el análisis sintáctico de la cadena de entrada
        /// </summary>
        public void Parse(string source)
        {
            parser.Parse(source);
        }

        // ========== MANEJADORES DE EVENTOS ========== //

        /// <summary>
        /// Evento: Cuando se lee un token
        /// </summary>
        private void TokenReadEvent(LALRParser parser, TokenReadEventArgs args)
        {
            try
            {
                // Asigna un valor semántico al token
                args.Token.UserObject = CreateObject(args.Token);
            }
            catch
            {
                args.Continue = false; // Aborta el parsing en caso de error
            }
        }

        /// <summary>
        /// Evento: Cuando se reduce una producción
        /// </summary>
        private void ReduceEvent(LALRParser parser, ReduceEventArgs args)
        {
            try
            {
                // Asigna un valor semántico al token reducido
                args.Token.UserObject = CreateObject(args.Token);
            }
            catch
            {
                args.Continue = false; // Aborta el parsing en caso de error
            }
        }

        /// <summary>
        /// Evento: Cuando se acepta la entrada
        /// </summary>
        private void AcceptEvent(LALRParser parser, AcceptEventArgs args)
        {
            // args.Token.UserObject contiene el resultado final del parsing
        }

        /// <summary>
        /// Evento: Error en token
        /// </summary>
        private void TokenErrorEvent(LALRParser parser, TokenErrorEventArgs args)
        {
            throw new SymbolException($"Error en token: '{args.Token}'");
        }

        /// <summary>
        /// Evento: Error de sintaxis
        /// </summary>
        private void ParseErrorEvent(LALRParser parser, ParseErrorEventArgs args)
        {
            throw new RuleException($"Error de sintaxis: '{args.UnexpectedToken}'");
        }

        // ========== MÉTODOS SEMÁNTICOS ========== //

        /// <summary>
        /// Crea objetos semánticos para tokens terminales
        /// </summary>
        private object? CreateObject(TerminalToken token)
        {
            return token.Symbol.Id switch
            {
                (int)SymbolConstants.SYMBOL_NUMBER => double.Parse(token.Text), // Convierte a número
                (int)SymbolConstants.SYMBOL_PI => Math.PI,                     // Constante PI
                (int)SymbolConstants.SYMBOL_MAS => "+",                        // Operador como string
                (int)SymbolConstants.SYMBOL_RESTA => "-",
                (int)SymbolConstants.SYMBOL_POR => "*",
                (int)SymbolConstants.SYMBOL_DIVIDIR => "/",
                (int)SymbolConstants.SYMBOL_SIN => "sin",
                (int)SymbolConstants.SYMBOL_RAIZ => "sqrt",
                _ => null // Valor por defecto para otros símbolos
            };
        }

        /// <summary>
        /// Crea objetos semánticos para tokens no terminales (aplicando reglas)
        /// </summary>
        public static object CreateObject(NonterminalToken token)
        {
            return token.Rule.Id switch
            {
                // Operaciones aritméticas
                (int)RuleConstants.RULE_E_MAS => (double)token.Tokens[0].UserObject! + (double)token.Tokens[2].UserObject!,
                (int)RuleConstants.RULE_E_RESTA => (double)token.Tokens[0].UserObject! - (double)token.Tokens[2].UserObject!,
                (int)RuleConstants.RULE_T_POR => (double)token.Tokens[0].UserObject! * (double)token.Tokens[2].UserObject!,
                (int)RuleConstants.RULE_T_DIVIDIR => (double)token.Tokens[0].UserObject! / (double)token.Tokens[2].UserObject!,
                
                // Funciones matemáticas
                (int)RuleConstants.RULE_F_SIN_LPAREN_RPAREN => Math.Sin((double)token.Tokens[2].UserObject!),
                (int)RuleConstants.RULE_F_RAIZ_LPAREN_RPAREN => Math.Sqrt((double)token.Tokens[2].UserObject!),
                
                // Valores literales
                (int)RuleConstants.RULE_F_PI => Math.PI,
                (int)RuleConstants.RULE_F_NUMBER => token.Tokens[0].UserObject!,
                
                _ => throw new RuleException("Regla gramatical no implementada")
            };
        }
    }
}