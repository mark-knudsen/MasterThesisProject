; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@str = private unnamed_addr constant [9 x i8] c"test.csv\00", align 1

define ptr @main_2() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  %unbox_gep = getelementptr inbounds nuw { i32, ptr }, ptr %arr_load, i32 0, i32 1
  %raw_array_ptr = load ptr, ptr %unbox_gep, align 8
  call void @ToCsvInternal(ptr %raw_array_ptr, ptr @str)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  %runtime_obj1 = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj1 to ptr
  %tag_ptr2 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 0, ptr %tag_ptr2, align 2
  %data_ptr3 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr3, align 8
  ret ptr %runtime_obj1
}

declare i32 @printf(ptr, ...)

declare void @ToCsvInternal(ptr, ptr)

declare ptr @malloc(i64)
