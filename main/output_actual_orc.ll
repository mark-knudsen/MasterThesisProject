; ModuleID = 'repl_module'
source_filename = "repl_module"

@str = private unnamed_addr constant [6 x i8] c"index\00", align 1
@str.1 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.2 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.3 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@str.4 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@str.5 = private unnamed_addr constant [4 x i8] c"Bob\00", align 1
@str.6 = private unnamed_addr constant [6 x i8] c"Alice\00", align 1
@str.7 = private unnamed_addr constant [5 x i8] c"John\00", align 1
@str.8 = private unnamed_addr constant [5 x i8] c"Mary\00", align 1
@df2 = global ptr null, align 8

define ptr @main_0() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 80)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 5, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 10, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr ptr, ptr %arr_data, i64 2
  store ptr @str.2, ptr %elem_ptr2, align 8
  %elem_ptr3 = getelementptr ptr, ptr %arr_data, i64 3
  store ptr @str.3, ptr %elem_ptr3, align 8
  %elem_ptr4 = getelementptr ptr, ptr %arr_data, i64 4
  store ptr @str.4, ptr %elem_ptr4, align 8
  %arr_header5 = call ptr @malloc(i64 24)
  %arr_data6 = call ptr @malloc(i64 64)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 0
  store i64 4, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 1
  store i64 8, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 2
  store ptr %arr_data6, ptr %5, align 8
  %record_buffer = call ptr @malloc(i64 40)
  %field_mem = call ptr @malloc(i64 8)
  store i64 0, ptr %field_mem, align 4
  %field_ptr = getelementptr ptr, ptr %record_buffer, i64 0
  store ptr %field_mem, ptr %field_ptr, align 8
  %field_ptr7 = getelementptr ptr, ptr %record_buffer, i64 1
  store ptr @str.5, ptr %field_ptr7, align 8
  %field_mem8 = call ptr @malloc(i64 8)
  store i64 23, ptr %field_mem8, align 4
  %field_ptr9 = getelementptr ptr, ptr %record_buffer, i64 2
  store ptr %field_mem8, ptr %field_ptr9, align 8
  %field_mem10 = call ptr @malloc(i64 8)
  store i1 true, ptr %field_mem10, align 1
  %field_ptr11 = getelementptr ptr, ptr %record_buffer, i64 3
  store ptr %field_mem10, ptr %field_ptr11, align 8
  %field_mem12 = call ptr @malloc(i64 8)
  store double 2.305000e+05, ptr %field_mem12, align 8
  %field_ptr13 = getelementptr ptr, ptr %record_buffer, i64 4
  store ptr %field_mem12, ptr %field_ptr13, align 8
  %elem_ptr14 = getelementptr ptr, ptr %arr_data6, i64 0
  store ptr %record_buffer, ptr %elem_ptr14, align 8
  %record_buffer15 = call ptr @malloc(i64 40)
  %field_mem16 = call ptr @malloc(i64 8)
  store i64 1, ptr %field_mem16, align 4
  %field_ptr17 = getelementptr ptr, ptr %record_buffer15, i64 0
  store ptr %field_mem16, ptr %field_ptr17, align 8
  %field_ptr18 = getelementptr ptr, ptr %record_buffer15, i64 1
  store ptr @str.6, ptr %field_ptr18, align 8
  %field_mem19 = call ptr @malloc(i64 8)
  store i64 23, ptr %field_mem19, align 4
  %field_ptr20 = getelementptr ptr, ptr %record_buffer15, i64 2
  store ptr %field_mem19, ptr %field_ptr20, align 8
  %field_mem21 = call ptr @malloc(i64 8)
  store i1 true, ptr %field_mem21, align 1
  %field_ptr22 = getelementptr ptr, ptr %record_buffer15, i64 3
  store ptr %field_mem21, ptr %field_ptr22, align 8
  %field_mem23 = call ptr @malloc(i64 8)
  store double 0x40F88948CCCCCCCD, ptr %field_mem23, align 8
  %field_ptr24 = getelementptr ptr, ptr %record_buffer15, i64 4
  store ptr %field_mem23, ptr %field_ptr24, align 8
  %elem_ptr25 = getelementptr ptr, ptr %arr_data6, i64 1
  store ptr %record_buffer15, ptr %elem_ptr25, align 8
  %record_buffer26 = call ptr @malloc(i64 40)
  %field_mem27 = call ptr @malloc(i64 8)
  store i64 2, ptr %field_mem27, align 4
  %field_ptr28 = getelementptr ptr, ptr %record_buffer26, i64 0
  store ptr %field_mem27, ptr %field_ptr28, align 8
  %field_ptr29 = getelementptr ptr, ptr %record_buffer26, i64 1
  store ptr @str.7, ptr %field_ptr29, align 8
  %field_mem30 = call ptr @malloc(i64 8)
  store i64 87, ptr %field_mem30, align 4
  %field_ptr31 = getelementptr ptr, ptr %record_buffer26, i64 2
  store ptr %field_mem30, ptr %field_ptr31, align 8
  %field_mem32 = call ptr @malloc(i64 8)
  store i1 false, ptr %field_mem32, align 1
  %field_ptr33 = getelementptr ptr, ptr %record_buffer26, i64 3
  store ptr %field_mem32, ptr %field_ptr33, align 8
  %field_mem34 = call ptr @malloc(i64 8)
  store double 0x413272A8051EB852, ptr %field_mem34, align 8
  %field_ptr35 = getelementptr ptr, ptr %record_buffer26, i64 4
  store ptr %field_mem34, ptr %field_ptr35, align 8
  %elem_ptr36 = getelementptr ptr, ptr %arr_data6, i64 2
  store ptr %record_buffer26, ptr %elem_ptr36, align 8
  %record_buffer37 = call ptr @malloc(i64 40)
  %field_mem38 = call ptr @malloc(i64 8)
  store i64 3, ptr %field_mem38, align 4
  %field_ptr39 = getelementptr ptr, ptr %record_buffer37, i64 0
  store ptr %field_mem38, ptr %field_ptr39, align 8
  %field_ptr40 = getelementptr ptr, ptr %record_buffer37, i64 1
  store ptr @str.8, ptr %field_ptr40, align 8
  %field_mem41 = call ptr @malloc(i64 8)
  store i64 29, ptr %field_mem41, align 4
  %field_ptr42 = getelementptr ptr, ptr %record_buffer37, i64 2
  store ptr %field_mem41, ptr %field_ptr42, align 8
  %field_mem43 = call ptr @malloc(i64 8)
  store i1 false, ptr %field_mem43, align 1
  %field_ptr44 = getelementptr ptr, ptr %record_buffer37, i64 3
  store ptr %field_mem43, ptr %field_ptr44, align 8
  %field_mem45 = call ptr @malloc(i64 8)
  store double 0x40C4E62000000000, ptr %field_mem45, align 8
  %field_ptr46 = getelementptr ptr, ptr %record_buffer37, i64 4
  store ptr %field_mem45, ptr %field_ptr46, align 8
  %elem_ptr47 = getelementptr ptr, ptr %arr_data6, i64 3
  store ptr %record_buffer37, ptr %elem_ptr47, align 8
  %arr_header48 = call ptr @malloc(i64 24)
  %arr_data49 = call ptr @malloc(i64 80)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 0
  store i64 5, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 1
  store i64 10, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 2
  store ptr %arr_data49, ptr %8, align 8
  %elem_ptr50 = getelementptr ptr, ptr %arr_data49, i64 0
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr50, align 8
  %elem_ptr51 = getelementptr ptr, ptr %arr_data49, i64 1
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr51, align 8
  %elem_ptr52 = getelementptr ptr, ptr %arr_data49, i64 2
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr52, align 8
  %elem_ptr53 = getelementptr ptr, ptr %arr_data49, i64 3
  store ptr bitcast (i64 3 to ptr), ptr %elem_ptr53, align 8
  %elem_ptr54 = getelementptr ptr, ptr %arr_data49, i64 4
  store ptr bitcast (i64 2 to ptr), ptr %elem_ptr54, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr ({ ptr, ptr, ptr }, ptr null, i32 1) to i32))
  %9 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df, i32 0, i32 0
  store ptr %arr_header, ptr %9, align 8
  %10 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df, i32 0, i32 1
  store ptr %arr_header5, ptr %10, align 8
  %11 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df, i32 0, i32 2
  store ptr %arr_header48, ptr %11, align 8
  store ptr %df, ptr @df2, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
