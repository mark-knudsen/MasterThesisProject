; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_name_age = type { ptr, i64 }

@z = external global ptr

define ptr @main_10() {
entry:
  %z_load = load ptr, ptr @z, align 8
  %z_load1 = load ptr, ptr @z, align 8
  %ptr_age = getelementptr inbounds nuw %struct_name_age, ptr %z_load1, i32 0, i32 1
  %val_age = load i64, ptr %ptr_age, align 4
  %value_mem = call ptr @malloc(i64 8)
  store i64 %val_age, ptr %value_mem, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 1, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
