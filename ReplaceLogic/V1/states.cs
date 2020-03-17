namespace ReplaceLogic.V1
{
    public enum states : /*s*/byte
    {
        //start = -1,
        read_first = 10,
        check_block = 20,
        next_char = 30,
        move_index = 35,
        fast_check_node = 40,
        check_node = 45,
        next_node = 50,
        check_char = 70,
        write_node = 80//,
        //end = 100
    }
}