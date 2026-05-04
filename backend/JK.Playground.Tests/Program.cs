// See https://aka.ms/new-console-template for more information

using System.Text;

Console.WriteLine("Hello, World!");

#region Reverse string (in-place idea)

static string Reverse(string input)
{
    var arr = input.ToCharArray();

    int left = 0, right = arr.Length - 1;

    while (left < right)
    {
        (arr[left], arr[right]) = (arr[right], arr[left]);
        left++;
        right--;
    }

    return new string(arr);
}

var reversed = Reverse("Hello World!");
Console.WriteLine(reversed);

#endregion

#region Find first non-repeating character

static char? FirstUnique(string input)
{
    var unique = new Dictionary<char, int>();

    foreach (var c in input)
        unique[c] = unique.GetValueOrDefault(c) + 1;

    var key = unique.FirstOrDefault(p => p.Value == 1).Key;
    return key != default ? key : null;
}

Console.WriteLine(FirstUnique("abraaabra"));

#endregion

#region Two Sum (array)
// don't know what it should do
static int[] TwoSum(int[] nums, int target) {
    Dictionary<int, int> dict = new();

    for (int i = 0; i < nums.Length; i++)
    {
        if (dict.TryGetValue(target - nums[i], out var j))
            return new[] { i, j };
        dict[nums[i]] = i;
    }
    return Array.Empty<int>();
}

var twoSum = TwoSum(new[] { 2, 7, 11, 15 }, 9);
if(twoSum.Length == 0)
    Console.WriteLine("No solution");
else
    Console.WriteLine($"{twoSum[0]}, { twoSum[1]}");
#endregion

#region Remove duplicates from sorted array (in-place)

// var l2 = new ListNode(564);
// var l1 = new ListNode(243, l2);
// Console.WriteLine(Solution.AddTwoNumbers(l1, l2).val);
//
// class ListNode {
//      public int val;
//      public ListNode next;
//      public ListNode(int val=0, ListNode next=null) {
//              this.val = val;
//              this.next = next;
//          }
//  }
//
// static class Solution
// {
//     public static ListNode AddTwoNumbers(ListNode l1, ListNode l2)
//     {
//
//         if (l1.val == 0 && l2.val == 0)
//             return new ListNode(0, l2);
//         if (l1.val != 0 && l2.val == 0)
//             return new ListNode(ReverseInt(l1.val), l2);
//         if (l1.val == 0 && l2.val != 0)
//             return new ListNode(ReverseInt(l2.val), l2);
//
//         var l1Val = ReverseInt(l1.val);
//         var l2Val = ReverseInt(l2.val);
//         var result = l1Val + l2Val;
//         Console.WriteLine($"{l1Val}, {l2Val}, {result}");
//
//         return new ListNode(ReverseInt(result), l2);
//     }
//
//     public static int ReverseInt(int num)
//     {
//         int result = 0;
//         while (num > 0)
//         {
//             result = result * 10 + num % 10;
//             num /= 10;
//         }
//
//         return result;
//     }
// }
#endregion

#region Longest Substring Without Repeating Characters

static int LengthOfLongestSubstring(string s)
{
    if(s.Length == 0)
        return 0;
    HashSet<string> unique = new HashSet<string>(s.Length);
    string current = "";

    for (int i = 0; i < s.Length; i++)
    {
        if(current.Contains(s[i]))
        {
            unique.Add(current);
            current = "";
        }
        current += s[i];
    }
    if(current.Any())
        unique.Add(current);
    Console.WriteLine(string.Join(", ", unique));
    return unique.Max(x => x.Length);
}


Console.WriteLine(LengthOfLongestSubstring("abcabcbb"));

#endregion

#region find longest palindromic substring
Console.WriteLine("Find longest palindromic substring");
Console.WriteLine("Starting:");
static string LongestPalindrome(string s)
{
    if (s.Length == 1)
        return s;

    int pStart = 0;
    bool startPalindrome = false;
    int pEnd = 0;
    bool isOdd = false;

    var pal = new List<string>(s.Length);

    for(int i = 1; i < s.Length; i++)
    {
        if (!startPalindrome)
        {
            if (s[i - 1] == s[i])
            {
                startPalindrome = true;
                pStart = i;
            }
            else if (i >= 2 && s[i - 2] == s[i])
            {
                startPalindrome = true;
                isOdd = true;
                pStart = i;
            }
        }
        else
        {
            var pLength = i - pStart;
            var startPal = (isOdd ? pStart - 1 : pStart) - pLength;
            if (startPal > 0 && s[startPal-1] == s[i])
                pEnd = i;
            else if (!isOdd && startPal >= 0 && s[startPal] == s[i])
            {
                pEnd = i;
                pStart = i;
                isOdd = true;
            }
            else
            {
                startPalindrome = false;
                pal.Add( s.Substring(startPal, 2*pLength+ (isOdd ? 1 : 0)));
                pStart = 0;
                pEnd = 0;

            }
        }
    }

    if (startPalindrome)
    {
        var pLength = s.Length - pStart;
        var startPal = (isOdd ? pStart - 1 : pStart) - pLength;
        pal.Add(s.Substring(startPal, 2 * pLength + (isOdd ? 1 : 0)));
    }

    if(pal.Count == 0)
        return s.Substring(0, 1);

    var max = pal.Max(x => x.Length);
    return pal.FirstOrDefault(x => x.Length == max, string.Empty);
}

static void CheckFromMiddle(string s, int left, int right, ref int start, ref int end, int maxLength)
{
    while (left >= 0 && right < s.Length && s[left] == s[right])
    {
        var currentLength = right - left + 1;
        if (currentLength > maxLength)
        {
            maxLength = currentLength;
            start = left;
            end = right;
        }

        left--;
        right++;
    }
}

Console.WriteLine("Result:");
// Console.WriteLine(LongestPalindrome("babad"));
// Console.WriteLine(LongestPalindrome("cbbd"));
// Console.WriteLine(LongestPalindrome("bb"));
// Console.WriteLine(LongestPalindrome("ccc"));
Console.WriteLine(LongestPalindrome("cccc"));
#endregion