Use BusTicketManagement

INSERT INTO Locations (Name, Address)
VALUES 
    (N'Hồ Tây', N'Phường Quảng An, Quận Tây Hồ, Hà Nội'),
    (N'Thảo Cầm Viên Sài Gòn', N'2 Nguyễn Bỉnh Khiêm, Quận 1, TP. Hồ Chí Minh'),
    (N'Cầu Rồng', N'Đường Trần Hưng Đạo, Quận Sơn Trà, Đà Nẵng'),
    (N'Vincom Mega Mall Royal City', N'72A Nguyễn Trãi, Quận Thanh Xuân, Hà Nội'),
    (N'Bitexco Financial Tower', N'2 Hải Triều, Quận 1, TP. Hồ Chí Minh'),
    (N'Bãi biển Mỹ Khê', N'Phước Mỹ, Quận Sơn Trà, Đà Nẵng'),
    (N'Lăng Chủ tịch Hồ Chí Minh', N'Số 2 Hùng Vương, Quận Ba Đình, Hà Nội'),
    (N'Tràng Tiền Plaza', N'24 Hai Bà Trưng, Quận Hoàn Kiếm, Hà Nội'),
    (N'Chợ Bến Thành', N'Đường Lê Lợi, Quận 1, TP. Hồ Chí Minh'),
    (N'Đồi Mộng Mơ', N'Số 5 Mai Anh Đào, Phường 8, TP. Đà Lạt'),
    (N'Tháp Rùa', N'Hồ Hoàn Kiếm, Quận Hoàn Kiếm, Hà Nội'),
    (N'Landmark 81', N'720A Điện Biên Phủ, Quận Bình Thạnh, TP. Hồ Chí Minh'),
    (N'Bảo tàng Chứng tích Chiến tranh', N'28 Võ Văn Tần, Quận 3, TP. Hồ Chí Minh'),
    (N'Chùa Thiên Mụ', N'Phường Hương Long, TP. Huế'),
    (N'Ga Đà Lạt', N'Số 1 Quang Trung, TP. Đà Lạt'),
    (N'Chợ Đà Lạt', N'Nguyễn Thị Minh Khai, Phường 1, TP. Đà Lạt'),
    (N'Cầu Tình Yêu', N'Đường Trần Hưng Đạo, Quận Sơn Trà, Đà Nẵng'),
    (N'Vincom Plaza Ngô Quyền', N'Vincom Plaza, Số 1 Lê Thánh Tông, Quận Ngô Quyền, Hải Phòng'),
    (N'Bãi Sau', N'Phường Thắng Tam, TP. Vũng Tàu'),
    (N'Bảo tàng Dân tộc học Việt Nam', N'Nguyễn Văn Huyên, Quận Cầu Giấy, Hà Nội'),
	(N'AEON Mall Tân Phú', N'30 Bờ Bao Tân Thắng, Quận Tân Phú, TP. Hồ Chí Minh'),
    (N'Bảo tàng Lịch sử Quốc gia', N'1 Tràng Tiền, Quận Hoàn Kiếm, Hà Nội'),
    (N'Chùa Một Cột', N'Số 1 Chùa Một Cột, Quận Ba Đình, Hà Nội'),
    (N'Vincom Center Đồng Khởi', N'72 Lê Thánh Tôn, Quận 1, TP. Hồ Chí Minh'),
    (N'Nhà hát Lớn Hà Nội', N'1 Tràng Tiền, Quận Hoàn Kiếm, Hà Nội'),
    (N'Nhà thờ Đức Bà Sài Gòn', N'01 Công Xã Paris, Quận 1, TP. Hồ Chí Minh'),
    (N'Công viên Thống Nhất', N'Đại Cồ Việt, Quận Hai Bà Trưng, Hà Nội'),
    (N'Công viên 23/9', N'Phạm Ngũ Lão, Quận 1, TP. Hồ Chí Minh'),
    (N'Tượng Phật Bà Quan Âm Bồ Tát', N'Chùa Linh Ứng, Bãi Bụt, Sơn Trà, Đà Nẵng'),
    (N'Chợ Hàn', N'119 Trần Phú, Quận Hải Châu, Đà Nẵng'),
    (N'Suối Tiên', N'120 Xa Lộ Hà Nội, Quận 9, TP. Hồ Chí Minh'),
    (N'Khu du lịch Bà Nà Hills', N'Thôn An Sơn, Xã Hòa Ninh, Huyện Hòa Vang, Đà Nẵng'),
    (N'Lotte Center Hà Nội', N'54 Liễu Giai, Quận Ba Đình, Hà Nội'),
    (N'Trung tâm Hội nghị Quốc gia', N'Số 1 Đại lộ Thăng Long, Quận Nam Từ Liêm, Hà Nội'),
    (N'Nhà tù Hỏa Lò', N'1 Hỏa Lò, Quận Hoàn Kiếm, Hà Nội'),
    (N'Vườn Quốc gia Cát Tiên', N'Tân Phú, Đồng Nai'),
    (N'Bến Ninh Kiều', N'Đường Hai Bà Trưng, Quận Ninh Kiều, TP. Cần Thơ'),
    (N'Bảo tàng Mỹ thuật TP.HCM', N'97A Phó Đức Chính, Quận 1, TP. Hồ Chí Minh'),
    (N'Chợ Đông Ba', N'1 Trần Hưng Đạo, TP. Huế'),
    (N'Thác Datanla', N'QL20, Phường 3, TP. Đà Lạt');

INSERT INTO VehicleTypes (Name, Price, TotalSeats, TotalFlooring, TotalRow, TotalColumn) VALUES
	('Mini Bus', 35000.00, 15, 1, 5, 3),
	('Standard Bus', 50000.00, 40, 1, 10, 4),
	('Double Decker', 85000.00, 70, 2, 10, 7),
	('Sleeper Bus', 75000.00, 36, 1, 9, 4),
	('Luxury Coach', 95000.00, 45, 1, 9, 5),
	('Shuttle Van', 30000.00, 12, 1, 4, 3),
	('Electric Bus', 88000.00, 35, 1, 7, 5);
	
Select * from VehicleTypes

Delete from VehicleTypes
Where Id between 8 And 14

INSERT INTO Vehicles (Name, LicensePlate, VehicleType) VALUES
	('Mini Express 01', 'ABC-1234', 'Mini Bus'),
	('City Runner', 'XYZ-5678', 'Standard Bus'),
	('Sky Deck', 'DD-9087', 'Double Decker'),
	('Night Comfort', 'SLP-4455', 'Sleeper Bus'),
	('Royal Cruiser', 'LUX-7788', 'Luxury Coach'),
	('Airport Shuttle 01', 'SHL-001', 'Shuttle Van'),
	('Eco Glide', 'ELE-2025', 'Electric Bus');

	Select * from Trips