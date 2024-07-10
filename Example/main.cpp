#include <iostream>
#include <thread>
#include "../DMALibrary/Memory/Memory.h"

int main()
{
    if (!mem.Init("cs2.exe", true, true))
    {
        std::cerr << "Failed to initialize DMA" << std::endl;
        return 1;
    }
    std::cout << "DMA initialized" << std::endl;

    uintptr_t base = mem.GetBaseDaddy("client.dll");

    //change this to false/true to turn on/off the features
    bool Spotted = true;

    //offsets
    uint64_t dwEntityList = 0x19BDD78;

    uint64_t m_hPlayerPawn = 0x7DC;
    uint64_t m_entitySpottedState = 0x2288;
    uint64_t m_bSpotted = 0x8;
    uint64_t m_lifeState = 0x328;
    uint64_t dwLocalPlayerPawn = 0x1823A08;
    uint64_t m_iTeamNum = 0x3C3;

    while (true)
    {
        //read LocalPlayer
        uint64_t LocalPlayer = mem.Read<uint64_t>(base + dwLocalPlayerPawn);

        //im rn to lazy to add team check
        //uint64_t localPlayerTeam = mem.Read<uint64_t>(dwLocalPlayerPawn + m_iTeamNum);


        uint64_t entityList = mem.Read<uint64_t>(base + dwEntityList);
        if (entityList == 0)
        {
            std::cerr << "Failed to read entity list" << std::endl;
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
            continue;
        }

        uint64_t listEntry = mem.Read<uint64_t>(entityList + 0x10);
        if (listEntry == 0)
        {
            std::cerr << "Failed to read first list entry" << std::endl;
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
            continue;
        }

        for (int i = 0; i < 64; i++)
        {
            if (listEntry == 0) // skip if invalid
                continue;

            uint64_t currentController = mem.Read<uint64_t>(listEntry + i * 0x78);
            if (currentController == 0)
                continue; // skip if invalid

            uint64_t pawnHandle = mem.Read<uint64_t>(currentController + m_hPlayerPawn);
            if (pawnHandle == 0)
                continue; // skip if invalid

            uint64_t listEntry2 = mem.Read<uint64_t>(entityList + 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
            if (listEntry2 == 0)
            {
                std::cerr << "Failed to read second list entry" << std::endl;
                continue;
            }

            uint64_t currentPawn = mem.Read<uint64_t>(listEntry2 + 0x78 * (pawnHandle & 0x1FF));
            if (currentPawn == 0)
            {
                std::cerr << "Failed to read current pawn" << std::endl;
                continue;
            }

            uint64_t lifestate = mem.Read<uint64_t>(currentPawn + m_lifeState);
            if (lifestate != 256)
            {
                continue;
            }
            if (currentPawn == LocalPlayer)
            {
                continue;
            }

            bool spottedstatus = mem.Read<bool>(currentPawn + m_entitySpottedState + m_bSpotted);
            if (Spotted == true)
            {
                if (spottedstatus == false)
                {
                    mem.Write<bool>(currentPawn + m_entitySpottedState + m_bSpotted, true);

                }
            }


        }
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }
}
