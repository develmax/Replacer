namespace ReplaceLogic.V2
{
    public enum states : sbyte
    {
        start = -1,
        read_first = 10,
        check_block = 20,
        move_index = 30,
        fast_check_node = 40,
        check_node = 45,
        next_node = 50,
        check_char = 70,
        write_node = 80,
        end = 100
    }
}