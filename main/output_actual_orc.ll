; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }

@str = private unnamed_addr constant [6 x i8] c"index\00", align 1
@str.1 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.2 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.3 = private unnamed_addr constant [4 x i8] c"dan\00", align 1
@str.4 = private unnamed_addr constant [6 x i8] c"alice\00", align 1
@x = global ptr null, align 8

define ptr @main_0() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 48)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 3, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 6, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr ptr, ptr %arr_data, i64 2
  store ptr @str.2, ptr %elem_ptr2, align 8
  %arr_header3 = call ptr @malloc(i64 24)
  %arr_data4 = call ptr @malloc(i64 32)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 0
  store i64 2, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 1
  store i64 4, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 2
  store ptr %arr_data4, ptr %5, align 8
  %record_buffer = call ptr @malloc(i64 24)
  %field_mem = call ptr @malloc(i64 8)
  store i64 0, ptr %field_mem, align 4
  %field_ptr = getelementptr ptr, ptr %record_buffer, i64 0
  store ptr %field_mem, ptr %field_ptr, align 8
  %field_ptr5 = getelementptr ptr, ptr %record_buffer, i64 1
  store ptr @str.3, ptr %field_ptr5, align 8
  %field_mem6 = call ptr @malloc(i64 8)
  store i64 30, ptr %field_mem6, align 4
  %field_ptr7 = getelementptr ptr, ptr %record_buffer, i64 2
  store ptr %field_mem6, ptr %field_ptr7, align 8
  %elem_ptr8 = getelementptr ptr, ptr %arr_data4, i64 0
  store ptr %record_buffer, ptr %elem_ptr8, align 8
  %record_buffer9 = call ptr @malloc(i64 24)
  %field_mem10 = call ptr @malloc(i64 8)
  store i64 1, ptr %field_mem10, align 4
  %field_ptr11 = getelementptr ptr, ptr %record_buffer9, i64 0
  store ptr %field_mem10, ptr %field_ptr11, align 8
  %field_ptr12 = getelementptr ptr, ptr %record_buffer9, i64 1
  store ptr @str.4, ptr %field_ptr12, align 8
  %field_mem13 = call ptr @malloc(i64 8)
  store i64 25, ptr %field_mem13, align 4
  %field_ptr14 = getelementptr ptr, ptr %record_buffer9, i64 2
  store ptr %field_mem13, ptr %field_ptr14, align 8
  %elem_ptr15 = getelementptr ptr, ptr %arr_data4, i64 1
  store ptr %record_buffer9, ptr %elem_ptr15, align 8
  %arr_header16 = call ptr @malloc(i64 24)
  %arr_data17 = call ptr @malloc(i64 48)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header16, i32 0, i32 0
  store i64 3, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header16, i32 0, i32 1
  store i64 6, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header16, i32 0, i32 2
  store ptr %arr_data17, ptr %8, align 8
  %elem_ptr18 = getelementptr ptr, ptr %arr_data17, i64 0
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr18, align 8
  %elem_ptr19 = getelementptr ptr, ptr %arr_data17, i64 1
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr19, align 8
  %elem_ptr20 = getelementptr ptr, ptr %arr_data17, i64 2
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr20, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %9 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header, ptr %9, align 8
  %10 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %arr_header3, ptr %10, align 8
  %11 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header16, ptr %11, align 8
  store ptr %df, ptr @x, align 8
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
