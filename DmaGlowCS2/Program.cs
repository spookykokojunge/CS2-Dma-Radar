using Framework;


//int swed 
Memory.init("cs2.exe", "client.dll");



UInt64 dwEntityList = 0x18C7F98;

UInt64 m_hPlayerPawn = 0x7E4;
UInt64 m_iszPlayerName = 0x638;
UInt64 m_entitySpottedState = 0x2278;
UInt64 m_bSpotted = 0x8;

//radar loop

while  (true)
{
    UInt64 entityList = Memory.Read<UInt64>(Offsets.GameAssembly + dwEntityList);

    //first entry
    UInt64 listEntry = Memory.Read<UInt64>(entityList + 0x10);

    //loop through controllers
    for (int i = 0; i < 64;i++)
    {
        if (listEntry == 0) //skip if invailed
            continue;

        UInt64 currentController = Memory.Read<UInt64>(listEntry + (ulong)(i * 0x78));

        if (currentController == 0)
            continue;//skip if invailed

        UInt64 pawnHandle = Memory.Read<UInt64>(currentController + m_hPlayerPawn);

        UInt64 listEntry2 = Memory.Read<UInt64>(entityList + 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        UInt64 currentPawn = Memory.Read<UInt64>(listEntry2 + 0x78 * (pawnHandle & 0x1FF));

        //get pawn attribute, incklude spotted for somereason i cant read this ?
        //string name = Memory.Read<string>(currentController + m_iszPlayerName + 16);

        bool spotted = Memory.Read<bool>(currentPawn + m_entitySpottedState + m_bSpotted);

        //write spotted status
        Memory.Write<bool>(currentPawn + m_entitySpottedState + m_bSpotted, true);

        string spoottedStatus = spotted == true ? "spotted" : " "; //string depending on if entity spotted

        Console.WriteLine($"{spoottedStatus}");
    }
    // sleep a bit 
    Thread.Sleep(100);
    Console.Clear();


}