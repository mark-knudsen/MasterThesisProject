; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_name_age = type { ptr, i64 }

@str = private unnamed_addr constant [4 x i8] c"dan\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"dan\00", align 1

define ptr @main_0() {
entry:
  %record_mem = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %ptr_name = getelementptr inbounds nuw %struct_name_age, ptr %record_mem, i32 0, i32 0
  store ptr @str.1, ptr %ptr_name, align 8
  %ptr_age = getelementptr inbounds nuw %struct_name_age, ptr %record_mem, i32 0, i32 1
  store i64 100, ptr %ptr_age, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 6, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %record_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i32)
