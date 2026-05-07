// 材質属性：アイテムやオブジェクトが「何でできているか」を表す属性
// 効果属性とは分ける。
// 例：金属は漏電や磁石などにも使えるし、木は燃える対象にもなり得る
public enum MaterialAttributeType
{
    None,
    Wood,   // 木
    Metal,  // 金属
    Stone,  // 石
    Soft    // 柔らかい素材
}
