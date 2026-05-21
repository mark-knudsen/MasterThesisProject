; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_ha_age = type { ptr, i64 }

@df2 = external global ptr
@str = private unnamed_addr constant [4 x i8] c"Bob\00", align 1

define ptr @main_4() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  %record_ptr = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_ha_age, ptr null, i32 1) to i64))
  %field_0 = getelementptr %struct_ha_age, ptr %record_ptr, i32 0, i32 0
  store ptr @str, ptr %field_0, align 8
  %field_1 = getelementptr %struct_ha_age, ptr %record_ptr, i32 0, i32 1
  store i64 22, ptr %field_1, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df2_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr, align 8
  %cap = load i64, ptr %cap_ptr, align 8
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %entry
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr, align 8
  store ptr %realloc, ptr %data_ptr_ptr, align 8
  br label %cont

cont:                                             ; preds = %grow, %entry
  %final_data_ptr = phi ptr [ %data, %entry ], [ %realloc, %grow ]
  %target_slot_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %record_ptr, ptr %target_slot_ptr, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %df2_load, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
