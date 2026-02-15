public struct Header
{
    public uint str_version; /* version number */
    public uint str_numstr; /* # of strings in the file */
    public uint str_longlen; /* length of longest string */
    public uint str_shortlen; /* shortest string length */
//define STR_RANDOM 0x1 /* randomized pointers */
//define STR_ORDERED 0x2 /* ordered pointers */
//define STR_ROTATED 0x4 /* rot-13'd text */
    public uint str_flags; /* bit field for flags */
    public char str_delim; /* delimiting character */
}