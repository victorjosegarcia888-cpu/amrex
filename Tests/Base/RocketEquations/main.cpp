#include <AMReX.H>
#include <AMReX_REAL.H>

#include <cmath>

namespace {

bool nearly_equal (amrex::Real a, amrex::Real b, amrex::Real tolerance)
{
    return std::abs(a - b) <= tolerance;
}

void test_mass_ratio_and_split ()
{
    constexpr amrex::Real oxidizer_flow = 510.0;
    constexpr amrex::Real fuel_flow = 140.0;
    constexpr amrex::Real total_flow = oxidizer_flow + fuel_flow;

    AMREX_ALWAYS_ASSERT(nearly_equal(total_flow, 650.0, 1.0e-12));
    AMREX_ALWAYS_ASSERT(nearly_equal(oxidizer_flow / fuel_flow,
                                     3.6428571428571428, 1.0e-12));
}

void test_circular_nozzle_area_ratio ()
{
    constexpr amrex::Real throat_diameter = 0.198;
    constexpr amrex::Real sea_level_exit_diameter = 0.62;
    constexpr amrex::Real vacuum_exit_diameter = 1.5;
    const amrex::Real sea_level_ratio =
        (sea_level_exit_diameter / throat_diameter) *
        (sea_level_exit_diameter / throat_diameter);
    const amrex::Real vacuum_ratio =
        (vacuum_exit_diameter / throat_diameter) *
        (vacuum_exit_diameter / throat_diameter);

    AMREX_ALWAYS_ASSERT(nearly_equal(sea_level_ratio, 9.8051229466, 1.0e-8));
    AMREX_ALWAYS_ASSERT(nearly_equal(vacuum_ratio, 57.3921028466, 1.0e-8));
}

void test_thrust_and_specific_impulse_relation ()
{
    constexpr amrex::Real mass_flow = 650.0;
    constexpr amrex::Real exit_velocity = 3000.0;
    constexpr amrex::Real exit_pressure = 120000.0;
    constexpr amrex::Real ambient_pressure = 101325.0;
    constexpr amrex::Real exit_area = 0.5;
    constexpr amrex::Real standard_gravity = 9.80665;
    const amrex::Real thrust = mass_flow * exit_velocity +
        (exit_pressure - ambient_pressure) * exit_area;
    const amrex::Real specific_impulse = thrust / (mass_flow * standard_gravity);
    const amrex::Real effective_exhaust_velocity = thrust / mass_flow;

    AMREX_ALWAYS_ASSERT(nearly_equal(specific_impulse,
                                     effective_exhaust_velocity / standard_gravity,
                                     1.0e-12));
    AMREX_ALWAYS_ASSERT(!nearly_equal(specific_impulse,
                                      exit_velocity / standard_gravity,
                                      1.0e-6));
}

}

int main (int argc, char* argv[])
{
    amrex::Initialize(argc, argv);

    test_mass_ratio_and_split();
    test_circular_nozzle_area_ratio();
    test_thrust_and_specific_impulse_relation();

    amrex::Finalize();
}