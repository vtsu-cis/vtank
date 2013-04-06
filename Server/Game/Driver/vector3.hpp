/*!
	\file vector3.hpp
	\brief Vector3 definition for 3D calculations.
	\author Copyright (C) 2010 by Vermont Technical College.
*/
//! Holds an (x, y, z) coordinate.
struct Vector3
{
	double x, y, z;
	
	Vector3(double v_x, double v_y, double v_z)
		: x(v_x), y(v_y), z(v_z) {}

	Vector3() : x(0), y(0), z(0) {}
};
