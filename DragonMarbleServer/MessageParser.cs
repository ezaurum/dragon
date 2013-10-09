using System.Text;

namespace DragonMarble
{
    public class MessageParser 
    {
        public void SetMessage(string message)
        {
            var dice = new StageDiceInfo();
            if ("D\n".Equals(message))
            {
                dice.Roll();
                int[] result = dice.result;
                byte[] messageBuffer = Encoding.UTF8.GetBytes(string.Format("{0},{1}\n", result[0], result[1]));
            }
            else if ("C\n".Equals(message))
            {
                dice.Clear();
            }
        }
    }
}